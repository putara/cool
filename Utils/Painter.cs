using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cool
{
    static class Painter
    {
        static public Image CreateDiagonalGradient(int size, Color firstColour, Color lastColour)
        {
            // turn over 45 degrees
            Point[] points =
                {
                    new Point(0, 0),
                    new Point(size, size)
                };
            // gradient colours stop at 0%, 50% and 100%.
            float[] positions = { 0, 1 };
            Color[] colours =
                {
                    firstColour,
                    lastColour
                };
            Rectangle rect = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = size,
                Height = size
            };

            // draw it.
            Bitmap bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            try
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    ColorBlend blend = new ColorBlend();
                    blend.Colors = colours;
                    blend.Positions = positions;
                    using (LinearGradientBrush br = new LinearGradientBrush(rect, firstColour, lastColour, 45))
                    {
                        br.InterpolationColors = blend;
                        g.FillRectangle(br, rect);
                    }
                }
                return bmp;
            }
            catch
            {
                bmp.Dispose();
                throw;
            }
        }

        static public Image CreateDiagonalGradient(int size, Color firstColour, Color midColour, Color lastColour)
        {
            // turn over 45 degrees
            Point[] points =
                {
                    new Point(0, 0),
                    new Point(size / 2, size / 2),
                    new Point(size, size)
                };
            // gradient colours stop at 0%, 50% and 100%.
            float[] positions = { 0, .5f, 1 };
            Color[] colours =
                {
                    firstColour,
                    midColour,
                    lastColour
                };
            Rectangle rect = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = size,
                Height = size
            };

            // draw it.
            Bitmap bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            try
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    ColorBlend blend = new ColorBlend();
                    blend.Colors = colours;
                    blend.Positions = positions;
                    using (LinearGradientBrush br = new LinearGradientBrush(rect, firstColour, lastColour, 45))
                    {
                        br.InterpolationColors = blend;
                        g.FillRectangle(br, rect);
                    }
                }
                return bmp;
            }
            catch
            {
                bmp.Dispose();
                throw;
            }
        }
    }
}
