using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Cool
{
    public class GraphicUtils
    {
        static Cursor handCursor;
        static Font defaultUIFont;

        internal static class NativeMethods
        {
            public const int IDC_HAND = 32649;
        }

        internal static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint="LoadCursorW")]
            public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);
        }

        static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID.Equals(format.Guid))
                {
                    return codec;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the real "Link Select" cursor.
        /// </summary>
        public static Cursor Hand
        {
            get
            {
                return System.Threading.LazyInitializer.EnsureInitialized<Cursor>(ref handCursor, () =>
                {
                    IntPtr handle = UnsafeNativeMethods.LoadCursor(IntPtr.Zero, (IntPtr)NativeMethods.IDC_HAND);
                    return new Cursor(handle);
                });
            }
        }

        /// <summary>
        /// Get the new default UI font containing emojis.
        /// </summary>
        public static Font DefaultUIFont
        {
            get
            {
                return System.Threading.LazyInitializer.EnsureInitialized<Font>(ref defaultUIFont, () =>
                {
                    // if the system has an emoji font, use it (win8+).
                    FontFamily found = new FontFamily("Segoe UI");
                    var families = new System.Drawing.Text.InstalledFontCollection().Families;
                    foreach (var family in families)
                    {
                        if (family.Name.Equals("Segoe UI Emoji"))
                        {
                            found = family;
                            break;
                        }
                    }
                    return new Font(found, 10.5F, FontStyle.Regular, GraphicsUnit.Point);
                });
            }
        }

        /// <summary>
        /// Create an Image from a byte array.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Bitmap ByteArrayToBitmap(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return null;
            }
            using (var stm = new MemoryStream(buffer))
            {
                try
                {
                    var image = Bitmap.FromStream(stm);
                    var bitmap = image as Bitmap;
                    if (bitmap == null)
                    {
                        image.Dispose();
                    }
                    return bitmap;
                }
                catch (ArgumentException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Save the image as a byte array.
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static byte[] BitmapToByteArray(Bitmap bmp, long quality = 90)
        {
            using (var stm = new MemoryStream())
            {
                var encoder = GetEncoder(ImageFormat.Jpeg);
                var encParams = new EncoderParameters(1);
                var param = new EncoderParameter(Encoder.Quality, quality);
                encParams.Param[0] = param;
                bmp.Save(stm, encoder, encParams);
                return stm.ToArray();
            }
        }

        public static Bitmap ShrinkBitmapIfNecessary(Bitmap srcImage, Size dstSize)
        {
            if (srcImage == null)
            {
                return null;
            }
            if (srcImage.Width <= dstSize.Width && srcImage.Height <= dstSize.Height)
            {
                return srcImage.Clone() as Bitmap;
            }
            var thumb = new Bitmap(dstSize.Width, dstSize.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(thumb))
            {
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(srcImage, new Rectangle(Point.Empty, dstSize), 0, 0, srcImage.Width, srcImage.Height, GraphicsUnit.Pixel);
            }
            return thumb;
        }
    }
}
