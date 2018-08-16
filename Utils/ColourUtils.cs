using System.Drawing;

namespace Cool
{
    static class ColourUtils
    {

        public static bool IsDark(Color colour)
        {
            return (colour.R * 2 + colour.G * 4 + colour.B) < 893;
        }

        public static Color Invert(Color colour)
        {
            var argb = colour.ToArgb();
            return Color.FromArgb(argb ^ 0xffffff);
        }

        public static Color Blend(Color baseColour, Color overlayColour, float alpha)
        {
            if (alpha == 0)
            {
                return overlayColour;
            }
            else if (alpha == 1)
            {
                return baseColour;
            }

            var ovlAlpha = 1f - alpha;
            return Color.FromArgb(
                    (int)(baseColour.R * alpha + overlayColour.R * ovlAlpha),
                    (int)(baseColour.G * alpha + overlayColour.G * ovlAlpha),
                    (int)(baseColour.B * alpha + overlayColour.B * ovlAlpha));
        }

        public static Color FromHSB(float hue, float saturation, float brightness)
        {
            // ref. https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSL
            float c = brightness * saturation;
            float h = hue * 6f;
            float x = c * (1 - Math.Abs(h % 2 - 1));
            float r, g, b;
            switch ((int)Math.Floor(h))
            {
                case 0:
                    r = c; g = x; b = 0; break;
                case 1:
                    r = x; g = c; b = 0; break;
                case 2:
                    r = 0; g = c; b = x; break;
                case 3:
                    r = 0; g = x; b = c; break;
                case 4:
                    r = x; g = 0; b = c; break;
                case 5:
                default:
                    r = c; g = 0; b = x; break;
            }
            float m = brightness - c;
            return Color.FromArgb(
                    (int)Math.Floor((r + m) * 255),
                    (int)Math.Floor((g + m) * 255),
                    (int)Math.Floor((b + m) * 255));
        }
    }
}
