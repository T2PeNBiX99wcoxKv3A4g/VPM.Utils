using JetBrains.Annotations;
using UnityEngine;

namespace io.github.ykysnk.utils.Extensions
{
    [PublicAPI]
    public static class Texture2DExtensions
    {
        public static Texture2D ScaleGPU(this Texture2D source, int targetWidth, int targetHeight)
        {
            var rt = new RenderTexture(targetWidth, targetHeight, 24);
            Graphics.Blit(source, rt);

            RenderTexture.active = rt;
            var result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            RenderTexture.active = null;
            rt.Release();

            return result;
        }

        public static Texture2D ScaleCPU(this Texture2D source, int targetWidth, int targetHeight)
        {
            var result = new Texture2D(targetWidth, targetHeight, source.format, false);

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
    }
}