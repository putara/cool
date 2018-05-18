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
    }
}
