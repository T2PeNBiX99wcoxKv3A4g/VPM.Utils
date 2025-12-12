using System;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class Texture2DExtensions
    {
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

        public static Texture2D ScaleCPU(this Texture2D source, int targetWidth, int targetHeight)
        {
            var result = new Texture2D(targetWidth, targetHeight, source.format, source.mipmapCount > 1);

            for (var y = 0; y < targetHeight; y++)
            for (var x = 0; x < targetWidth; x++)
            {
                var u = (float)x / targetWidth;
                var v = (float)y / targetHeight;
                var color = source.GetPixelBilinear(u, v);
                result.SetPixel(x, y, color);
            }

            result.Apply();
            return result;
        }

        // Refs: https://github.com/weasel-club/OneClickInventory/blob/main/Editor/Util/IconUtil.cs#L146
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

        public static Texture2D TrimTransparentCPU(this Texture2D source, float alphaThreshold = 0.01f)
        {
            int minX = source.width, minY = source.height;
            int maxX = 0, maxY = 0;

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

            var size = Mathf.Max(maxX - minX, maxY - minY);
            if (size < 0)
                size = 1;

            var trimmedSizeX = maxX - minX;
            var trimmedSizeY = maxY - minY;
            var trimmedPixels = source.GetPixels(minX, minY, trimmedSizeX, trimmedSizeY);
            var trimmed = new Texture2D(size, size, source.format, source.mipmapCount > 1);
            MakeTexture2DClear(trimmed, size, size);
            trimmed.SetPixels(size / 2 - trimmedSizeX / 2, size / 2 - trimmedSizeY / 2, trimmedSizeX, trimmedSizeY,
                trimmedPixels);
            trimmed.Apply();
            return trimmed;
        }

        public static Texture2D TrimTransparentGPU(this Texture2D source, float alphaThreshold = 0.01f,
            bool gammaCorrect = true, ComputeShader trimShader = null)
        {
            if (trimShader == null)
#if UNITY_EDITOR
                trimShader = TrimShader;
#else
            {
                var empty = new Texture2D(1, 1, source.format, source.mipmapCount > 1);
                empty.SetPixel(0, 0, Color.clear);
                empty.Apply();
                return empty;
            }
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
                var empty = new Texture2D(1, 1, source.format, source.mipmapCount > 1);
                empty.SetPixel(0, 0, Color.clear);
                empty.Apply();
                boundsBuffer.Release();
                return empty;
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
        private static readonly int Source = Shader.PropertyToID("source");
        private static readonly int Bounds = Shader.PropertyToID("bounds");
        private static readonly int AlphaThreshold = Shader.PropertyToID("alpha_threshold");
        private static readonly int Width = Shader.PropertyToID("width");
        private static readonly int Height = Shader.PropertyToID("height");
        private static readonly int Out = Shader.PropertyToID("_Out");
        private static readonly int GammaCorrect = Shader.PropertyToID("gamma_correct");

        private static ComputeShader TrimShader => _trimShader = _trimShader ??
                                                                 AssetDatabase.LoadAssetAtPath<ComputeShader>(
                                                                     AssetDatabase.GUIDToAssetPath(TrimShaderGuid));
#endif
    }
}