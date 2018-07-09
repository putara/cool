using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;

namespace Cool
{
    public enum AnnotationIcon
    {
        None,
        Application,
        Error,
        Exclamation,
        Information,
        Question,
        Shield,
        WinLogo,

        Asterisk,   // = Information
        Hand,       // = Error
        Warning     // = Exclamation
    }

    public class AnnotationLabel : Label, IDisposable
    {
        private const int CXY_PADDING = 2;
        public static readonly Padding INNER_PADDING_DEFAULT = new Padding(CXY_PADDING);

        AnnotationIcon iconType = AnnotationIcon.None;
        Color backColour = Color.Empty;
        Color foreColour = Color.Empty;
        Icon cachedIcon;
        Padding innerPadding = INNER_PADDING_DEFAULT;

        static class NativeMethods
        {
            public const int IDI_APPLICATION = 32512;
            public const int IDI_HAND = 32513;
            public const int IDI_QUESTION = 32514;
            public const int IDI_EXCLAMATION = 32515;
            public const int IDI_ASTERISK = 32516;
            public const int IDI_WINLOGO = 32517;
            public const int IDI_SHIELD = 32518;

            public const uint IMAGE_ICON = 1;
            public const uint LR_COPYFROMRESOURCE = 0x4000;
        }

        static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern int DestroyIcon(IntPtr hico);

            [DllImport("user32.dll", EntryPoint = "LoadIconW", CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern IntPtr CopyImage(IntPtr h, uint type, int cx, int cy, uint flags);
        }

        public AnnotationLabel()
        {
            base.TextAlign = ContentAlignment.MiddleLeft;

            ResetPadding();
            ResetBackColor();
            ResetForeColor();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.cachedIcon != null)
            {
                this.cachedIcon.Dispose();
                this.cachedIcon = null;
            }
        }

        [
            DefaultValue(AnnotationIcon.None)
        ]
        public AnnotationIcon AnnotationIcon
        {
            get
            {
                return this.iconType;
            }
            set
            {
                this.iconType = value;
                ReloadIcon();
            }
        }

        public new Color BackColor
        {
            get
            {
                if (this.backColour.IsEmpty)
                {
                    return SystemColors.Info;
                }
                return this.backColour;
            }
            set
            {
                this.backColour = value;
                base.BackColor = this.BackColor;
            }
        }

        public override void ResetBackColor()
        {
            this.BackColor = Color.Empty;
        }

        internal bool ShouldSerializeBackColor()
        {
            return this.backColour.IsEmpty == false;
        }

        public new Color ForeColor
        {
            get
            {
                if (this.foreColour.IsEmpty)
                {
                    return SystemColors.InfoText;
                }
                return this.foreColour;
            }
            set
            {
                this.foreColour = value;
                base.ForeColor = this.ForeColor;
            }
        }

        public override void ResetForeColor()
        {
            this.ForeColor = Color.Empty;
        }

        internal bool ShouldSerializeForeColor()
        {
            return this.foreColour.IsEmpty == false;
        }

        public new Padding Padding
        {
            get
            {
                return base.Padding - this.innerPadding;
            }
            set
            {
                base.Padding = value + this.innerPadding;
            }
        }

        public void ResetPadding()
        {
            this.Padding = Padding.Empty;
        }

        internal bool ShouldSerializePadding()
        {
            return this.Padding != Padding.Empty;
        }

        protected override Padding DefaultPadding
        {
            get
            {
                return Padding.Empty;
            }
        }

        [
            DefaultValue(ContentAlignment.MiddleLeft)
        ]
        public new ContentAlignment TextAlign
        {
            get
            {
                return base.TextAlign;
            }
            set
            {
                base.TextAlign = value;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.cachedIcon != null)
            {
                int y;
                switch (base.TextAlign)
                {
                    case ContentAlignment.BottomLeft:
                    case ContentAlignment.BottomRight:
                    case ContentAlignment.BottomCenter:
                        y = this.Height - CXY_PADDING - this.cachedIcon.Height;
                        break;
                    case ContentAlignment.MiddleLeft:
                    case ContentAlignment.MiddleRight:
                    case ContentAlignment.MiddleCenter:
                        y = (this.Height - this.cachedIcon.Height + 1) / 2;
                        break;
                    default:
                        y = CXY_PADDING;
                        break;
                }
                e.Graphics.DrawIcon(this.cachedIcon, CXY_PADDING + this.Padding.Left, y + this.Padding.Top);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateLayout();
        }

        protected override void OnAutoSizeChanged(EventArgs e)
        {
            base.OnAutoSizeChanged(e);
            UpdateLayout();
        }

        /// <summary>
        /// Recalculate padding sizes.
        /// </summary>
        void UpdateLayout()
        {
            this.SuspendLayout();
            var padding = this.Padding;

            this.innerPadding = INNER_PADDING_DEFAULT;

            if (this.cachedIcon != null)
            {
                // TODO: support RTL languages
                this.innerPadding.Left += this.cachedIcon.Width;

                var cyActual = this.Height - base.Padding.Vertical;
                var cyMin = this.cachedIcon.Height;
                if (this.AutoSize && cyActual < cyMin)
                {
                    this.innerPadding.Top += (cyMin - cyActual) / 2;
                    this.innerPadding.Bottom += (cyMin - cyActual + 1) / 2;
                }
            }

            // this will refresh the internal padding state of the Label control
            this.Padding = padding;
            this.ResumeLayout(true);
            this.Invalidate();
        }

        /// <summary>
        /// Reload cached icon.
        /// </summary>
        void ReloadIcon()
        {
            if (this.cachedIcon != null)
            {
                this.cachedIcon.Dispose();
                this.cachedIcon = null;
            }

            int resId = ResourceIdFromKnownIconType(this.iconType);
            if (resId != 0)
            {
                IntPtr hico = UnsafeNativeMethods.LoadIcon(IntPtr.Zero, new IntPtr(resId));
                if (hico != IntPtr.Zero)
                {
                    var smallSize = SystemInformation.SmallIconSize;
                    IntPtr hicoSmall = UnsafeNativeMethods.CopyImage(hico, NativeMethods.IMAGE_ICON, smallSize.Width, smallSize.Height, NativeMethods.LR_COPYFROMRESOURCE);
                    if (hicoSmall != IntPtr.Zero)
                    {
                        var icon = Icon.FromHandle(hicoSmall);
                        this.cachedIcon = new Icon(icon, smallSize);
                        UnsafeNativeMethods.DestroyIcon(hicoSmall);
                    }
                    UnsafeNativeMethods.DestroyIcon(hico);
                }
            }

            UpdateLayout();
        }

        /// <summary>
        /// Convert AnnotationIcon value to Win32 system icon ID.
        /// </summary>
        /// <param name="icon">The value of AnnotationIcon property.</param>
        /// <returns>The Icon ID.</returns>
        static int ResourceIdFromKnownIconType(AnnotationIcon icon)
        {
            switch (icon)
            {
                case AnnotationIcon.None:
                    return 0;

                case AnnotationIcon.Application:
                    return NativeMethods.IDI_APPLICATION;

                case AnnotationIcon.Error:
                case AnnotationIcon.Hand:
                    return NativeMethods.IDI_HAND;

                case AnnotationIcon.Exclamation:
                case AnnotationIcon.Warning:
                    return NativeMethods.IDI_EXCLAMATION;

                case AnnotationIcon.Information:
                case AnnotationIcon.Asterisk:
                    return NativeMethods.IDI_ASTERISK;

                case AnnotationIcon.Question:
                    return NativeMethods.IDI_QUESTION;

                case AnnotationIcon.Shield:
                    return NativeMethods.IDI_SHIELD;

                case AnnotationIcon.WinLogo:
                    return NativeMethods.IDI_WINLOGO;
            }

            Debug.Fail("Not coming here.");
            return 0;
        }
    }
}
