using System;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace io.github.ykysnk.utils.Extensions
{
    /// <summary>
    ///     Texture resizing and alpha trimming helpers. Trimming returns a square texture sized to the longer edge of the
    ///     opaque area, with the content centred and the remaining margin transparent.
    /// </summary>
    [PublicAPI]
    public static class Texture2DExtensions
    {
        // Shader property names of TrimTransparent.compute, hashed once here instead of on every dispatch.
        private static readonly int Source = Shader.PropertyToID("source");
        private static readonly int Bounds = Shader.PropertyToID("bounds");
        private static readonly int AlphaThreshold = Shader.PropertyToID("alpha_threshold");
        private static readonly int Width = Shader.PropertyToID("width");
        private static readonly int Height = Shader.PropertyToID("height");
        private static readonly int Out = Shader.PropertyToID("_Out");
        private static readonly int GammaCorrect = Shader.PropertyToID("gamma_correct");

        /// <summary>
        ///     Resizes the texture on the GPU: blits <paramref name="source" /> into a RenderTexture and reads it back into
        ///     a new Texture2D.
        /// </summary>
        /// <param name="source">The texture to resample; sampled through the GPU, so it does not need to be CPU readable.</param>
        /// <param name="targetWidth">Width of the returned texture in pixels; must be positive.</param>
        /// <param name="targetHeight">Height of the returned texture in pixels; must be positive.</param>
        /// <returns>
        ///     A new texture in the source format, with mipmaps only when the source had them. RenderTexture.active is left
        ///     at null afterwards rather than restored.
        /// </returns>
        public static Texture2D ScaleGPU(this Texture2D source, int targetWidth, int targetHeight)
        {
            var rt = new RenderTexture(targetWidth, targetHeight, 24, source.graphicsFormat);
            Graphics.Blit(source, rt);

            RenderTexture.active = rt;
            var result = new Texture2D(targetWidth, targetHeight, source.format, source.mipmapCount > 1);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();
            RenderTexture.active = null;
            rt.Release();

            return result;
        }

        /// <summary>
        ///     Resizes the texture on the CPU, writing one bilinearly sampled pixel at a time.
        /// </summary>
        /// <param name="source">The texture to sample; must be CPU readable and is not modified.</param>
        /// <param name="targetWidth">Width of the returned texture in pixels; must be positive.</param>
        /// <param name="targetHeight">Height of the returned texture in pixels; must be positive.</param>
        /// <returns>
        ///     A new texture in the source format, with mipmaps only when the source had them. Sampling happens at pixel
        ///     centres, so the first and last source pixels are reached.
        /// </returns>
        public static Texture2D ScaleCPU(this Texture2D source, int targetWidth, int targetHeight)
        {
            var result = new Texture2D(targetWidth, targetHeight, source.format, source.mipmapCount > 1);

            for (var y = 0; y < targetHeight; y++)
            for (var x = 0; x < targetWidth; x++)
            {
                var u = (x + 0.5f) / targetWidth;
                var v = (y + 0.5f) / targetHeight;
                var color = source.GetPixelBilinear(u, v);
                result.SetPixel(x, y, color);
            }

            result.Apply();
            return result;
        }

        // Refs: https://github.com/weasel-club/OneClickInventory/blob/main/Editor/Util/IconUtil.cs#L146
        /// <summary>
        ///     Fills the whole texture with Color.clear (0, 0, 0, 0) by writing a single full-size pixel array, which is
        ///     cheaper than a per-pixel loop. Apply is not called, so the change only reaches the GPU once the caller
        ///     applies the texture.
        /// </summary>
        /// <param name="tex2D">The texture to overwrite; it is mutated in place.</param>
        /// <param name="width">Texture width in pixels; must match the texture's current width or SetPixels throws.</param>
        /// <param name="height">Texture height in pixels; must match the texture's current height or SetPixels throws.</param>
        public static void MakeTexture2DClear(this Texture2D tex2D, int width, int height)
        {
            var clearColors = new Color[width * height];

            var initialBlock = new Color[width];
            for (var i = 0; i < width; i++)
                initialBlock[i] = Color.clear;

            var remaining = clearColors.Length;
            var copyPos = 0;
            while (remaining > 0)
            {
                var copyLength = Mathf.Min(width, remaining);
                Array.Copy(initialBlock, 0, clearColors, copyPos, copyLength);
                remaining -= copyLength;
                copyPos += copyLength;
            }

            tex2D.SetPixels(clearColors);
        }

        /// <summary>
        ///     Crops the transparent border away on the CPU. A pixel counts as content when its alpha is strictly greater
        ///     than <paramref name="alphaThreshold" />; the bounding box of that content is copied into a new square texture
        ///     whose side is the longer edge of the box, centred with integer division so an odd difference leaves one extra
        ///     transparent pixel on the right and top. The square output loses the original position of the content.
        /// </summary>
        /// <param name="source">The texture to crop; must be CPU readable and is not modified.</param>
        /// <param name="alphaThreshold">
        ///     Alpha above which a pixel is content, in the 0..1 range; the comparison is strict. Defaults to 0.01.
        /// </param>
        /// <returns>
        ///     A new texture in the source format, with mipmaps only when the source had them, or a 1x1 transparent texture
        ///     when no pixel passes the threshold.
        /// </returns>
        public static Texture2D TrimTransparentCPU(this Texture2D source, float alphaThreshold = 0.01f)
        {
            int minX = source.width, minY = source.height;
            int maxX = -1, maxY = -1;

            var pixels = source.GetPixels();

            for (var y = 0; y < source.height; y++)
            for (var x = 0; x < source.width; x++)
            {
                var c = pixels[y * source.width + x];
                if (!(c.a > alphaThreshold)) continue;
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }

            if (maxX < minX || maxY < minY) return CreateClearTexture(source);

            var trimmedSizeX = maxX - minX + 1;
            var trimmedSizeY = maxY - minY + 1;
            var size = Mathf.Max(trimmedSizeX, trimmedSizeY);

            var trimmedPixels = source.GetPixels(minX, minY, trimmedSizeX, trimmedSizeY);
            var trimmed = new Texture2D(size, size, source.format, source.mipmapCount > 1);
            trimmed.MakeTexture2DClear(size, size);
            trimmed.SetPixels(size / 2 - trimmedSizeX / 2, size / 2 - trimmedSizeY / 2, trimmedSizeX, trimmedSizeY,
                trimmedPixels);
            trimmed.Apply();
            return trimmed;
        }

        private static Texture2D CreateClearTexture(Texture2D source)
        {
            var empty = new Texture2D(1, 1, source.format, source.mipmapCount > 1);
            empty.SetPixel(0, 0, Color.clear);
            empty.Apply();
            return empty;
        }

        /// <summary>
        ///     GPU version of <see cref="TrimTransparentCPU" /> with the same square output: one kernel finds the alpha
        ///     bounds, a second copies that region into a square RenderTexture which is read back into a new texture.
        ///     Content is centred with integer division and everything outside it is written as transparent.
        /// </summary>
        /// <param name="source">The texture to crop; sampled on the GPU, so it does not need to be CPU readable.</param>
        /// <param name="alphaThreshold">
        ///     Alpha above which a pixel is content, in the 0..1 range; the comparison is strict. Defaults to 0.01.
        /// </param>
        /// <param name="gammaCorrect">
        ///     True (default) encodes the copied colour with pow(rgb, 1 / 2.2); false copies the raw source values.
        /// </param>
        /// <param name="trimShader">
        ///     Compute shader providing the cs_find_bounds and cs_copy_to_square kernels. When null, the editor loads the
        ///     shader shipped with this package and player builds return a 1x1 transparent texture instead.
        /// </param>
        /// <returns>
        ///     A new texture in the source format, with mipmaps only when the source had them, or a 1x1 transparent texture
        ///     when no pixel passes the threshold.
        /// </returns>
        public static Texture2D TrimTransparentGPU(this Texture2D source, float alphaThreshold = 0.01f,
            bool gammaCorrect = true, ComputeShader trimShader = null)
        {
            if (trimShader == null)
#if UNITY_EDITOR
                trimShader = TrimShader;
#else
                return CreateClearTexture(source);
#endif
            var kernelFind = trimShader.FindKernel("cs_find_bounds");
            var kernelCopy = trimShader.FindKernel("cs_copy_to_square");

            var boundsBuffer = new ComputeBuffer(4, sizeof(int));
            int[] initBounds =
            {
                int.MaxValue, int.MaxValue, -1, -1
            };
            boundsBuffer.SetData(initBounds);

            trimShader.SetTexture(kernelFind, Source, source);
            trimShader.SetBuffer(kernelFind, Bounds, boundsBuffer);
            trimShader.SetFloat(AlphaThreshold, alphaThreshold);
            trimShader.SetInt(Width, source.width);
            trimShader.SetInt(Height, source.height);

            var gx = Mathf.CeilToInt(source.width / 8f);
            var gy = Mathf.CeilToInt(source.height / 8f);
            trimShader.Dispatch(kernelFind, gx, gy, 1);

            var bounds = new int[4];
            boundsBuffer.GetData(bounds);
            int minX = bounds[0], minY = bounds[1], maxX = bounds[2], maxY = bounds[3];

            if (maxX < minX || maxY < minY)
            {
                boundsBuffer.Release();
                return CreateClearTexture(source);
            }

            var trimmedW = maxX - minX + 1;
            var trimmedH = maxY - minY + 1;
            var size = Mathf.Max(trimmedW, trimmedH);

            var rt = new RenderTexture(size, size, 24, source.graphicsFormat)
            {
                enableRandomWrite = true
            };
            rt.Create();

            trimShader.SetTexture(kernelCopy, Source, source);
            trimShader.SetTexture(kernelCopy, Out, rt);
            trimShader.SetBuffer(kernelCopy, Bounds, boundsBuffer);
            trimShader.SetInt(GammaCorrect, gammaCorrect ? 1 : 0);

            var cgx = Mathf.CeilToInt(size / 8f);
            var cgy = Mathf.CeilToInt(size / 8f);
            trimShader.Dispatch(kernelCopy, cgx, cgy, 1);

            var result = new Texture2D(size, size, source.format, source.mipmapCount > 1);
            RenderTexture.active = rt;
            result.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            result.Apply();
            RenderTexture.active = null;
            rt.Release();
            boundsBuffer.Release();

            return result;
        }
#if UNITY_EDITOR
        private const string TrimShaderGuid = "adc17c90949f49fb9d5413d8487e5d32";
        private static ComputeShader _trimShader;

        // Loaded by GUID because a static class cannot hold a serialized asset reference; the shader stays editor-only,
        // which is why the player build path above bails out without a caller-supplied shader.
        private static ComputeShader TrimShader => _trimShader = _trimShader ??
                                                                 AssetDatabase.LoadAssetAtPath<ComputeShader>(
                                                                     AssetDatabase.GUIDToAssetPath(TrimShaderGuid));
#endif
    }
}