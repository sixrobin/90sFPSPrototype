using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [AddComponentMenu("Image Effects/Color Adjustments/Grayscale")]
    public class Grayscale : ImageEffectBase
    {
        public Texture2D textureRamp;

        [Range(-1.0f, 1.0f)]
        public float rampOffset;

        public bool inverted;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            material.SetTexture("_RampTex", inverted ? textureRamp.FlipTexture() : textureRamp);
            material.SetFloat("_RampOffset", rampOffset);

            Graphics.Blit(source, destination, material);
        }
    }

    public static class Texture2DExtensions
    {
        public static Texture2D FlipTexture(this Texture2D original)
        {
            Texture2D flipped = new Texture2D(original.width, original.height)
            {
                wrapModeU = TextureWrapMode.Clamp
            };

            int w = original.width;
            int h = original.height;

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    flipped.SetPixel(w - x - 1, y, original.GetPixel(x, y));

            flipped.Apply();

            return flipped;
        }
    }
}
