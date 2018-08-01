using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;

namespace Cool
{
    public enum AnnotationType
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

    public enum AnnotationIcon
    {
        None,
        Small,
        Large
    }

    public class AnnotationLabel : Label, IDisposable
    {
        private const int CXY_PADDING = 2;
        private static readonly Padding INNER_PADDING_DEFAULT = new Padding(CXY_PADDING);
        private const AnnotationType TYPE_DEFAULT = AnnotationType.None;
        private const AnnotationIcon ICON_DEFAULT = AnnotationIcon.Small;
        private const TextImageRelation TIR_DEFAULT = TextImageRelation.ImageBeforeText;

        private AnnotationType annotationType = TYPE_DEFAULT;
        private AnnotationIcon annotationIcon = ICON_DEFAULT;
        private TextImageRelation textImageRelation = TIR_DEFAULT;
        private bool autoColour = false;
        private Color backColour = Color.Empty;
        private Color foreColour = Color.Empty;
        private Icon cachedIcon;
        private Padding innerPadding = INNER_PADDING_DEFAULT;

        private static class NativeMethods
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

        private static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern int DestroyIcon(IntPtr hico);

            [DllImport("user32.dll", EntryPoint = "LoadIconW", CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern IntPtr CopyImage(IntPtr h, uint type, int cx, int cy, uint flags);

            [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode)]
            public static extern int ExtractIconEx(string lpszFile, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIcons);
        }

        public AnnotationLabel()
        {
            base.TextAlign = ContentAlignment.MiddleLeft;

            this.ResetPadding();
            this.ResetBackColor();
            this.ResetForeColor();
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
            Category(CategoryNames.Appearance),
            DefaultValue(TYPE_DEFAULT)
        ]
        public AnnotationType AnnotationType
        {
            get
            {
                return this.annotationType;
            }
            set
            {
                if (this.annotationType != value)
                {
                    this.annotationType = value;
                    ReloadIcon();
                    if (this.autoColour)
                    {
                        this.ResetBackColor();
                        this.ResetForeColor();
                    }
                }
            }
        }

        [
            Category(CategoryNames.Appearance),
            DefaultValue(TIR_DEFAULT)
        ]
        public TextImageRelation TextImageRelation
        {
            get
            {
                return this.textImageRelation;
            }
            set
            {
                if (value != TextImageRelation.ImageBeforeText && value != TextImageRelation.TextBeforeImage)
                {
                    throw new ArgumentException("Only ImageBeforeText or TextBeforeImage are allowed.");
                }
                if (value != this.textImageRelation)
                {
                    this.textImageRelation = value;
                    UpdateLayout();
                }
            }
        }

        [
            Category(CategoryNames.Appearance),
            DefaultValue(ICON_DEFAULT)
        ]
        public AnnotationIcon AnnotationIcon
        {
            get
            {
                return this.annotationIcon;
            }
            set
            {
                if (this.annotationIcon != value)
                {
                    this.annotationIcon = value;
                    if (this.annotationType != AnnotationType.None)
                    {
                        ReloadIcon();
                    }
                }
            }
        }

        [
            Category(CategoryNames.Appearance),
            DefaultValue(false)
        ]
        public bool AutoColour
        {
            get
            {
                return this.autoColour;
            }
            set
            {
                if (this.autoColour != value)
                {
                    var oldValue = this.autoColour;
                    this.autoColour = value;

                    if (value || oldValue)
                    {
                        this.ResetBackColor();
                        this.ResetForeColor();
                    }
                }
            }
        }

        public new Color BackColor
        {
            get
            {
                if (this.backColour.IsEmpty)
                {
                    Color bg, fg;
                    ColourFromKnownKnownAnnotationType(this.autoColour ? this.annotationType : AnnotationType.None, out bg, out fg);
                    return bg;
                }
                return this.backColour;
            }
            set
            {
                if (value.IsEmpty == false)
                {
                    this.AutoColour = false;
                }

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
                    Color bg, fg;
                    ColourFromKnownKnownAnnotationType(this.autoColour ? this.annotationType : AnnotationType.None, out bg, out fg);
                    return fg;
                }
                return this.foreColour;
            }
            set
            {
                if (value.IsEmpty == false)
                {
                    this.AutoColour = false;
                }

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
                var x = CXY_PADDING + this.Padding.Left;
                if (this.IconIsLeft == false)
                {
                    x = this.Width - this.cachedIcon.Width - this.Padding.Right - CXY_PADDING;
                }
                e.Graphics.DrawIcon(this.cachedIcon, x, y + this.Padding.Top);
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

        private bool IconIsLeft
        {
            get
            {
                return this.RightToLeft == RightToLeft.Yes
                    ? this.TextImageRelation != TextImageRelation.ImageBeforeText
                    : this.textImageRelation == TextImageRelation.ImageBeforeText;
            }
        }

        /// <summary>
        /// Recalculate padding sizes.
        /// </summary>
        private void UpdateLayout()
        {
            this.SuspendLayout();
            var padding = this.Padding;

            this.innerPadding = INNER_PADDING_DEFAULT;

            if (this.cachedIcon != null)
            {
                if (this.IconIsLeft)
                {
                    this.innerPadding.Left += this.cachedIcon.Width;
                }
                else
                {
                    this.innerPadding.Right += this.cachedIcon.Width;
                }

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
        private void ReloadIcon()
        {
            if (this.cachedIcon != null)
            {
                this.cachedIcon.Dispose();
                this.cachedIcon = null;
            }

            this.cachedIcon = LoadIconFromKnownAnnotationType(this.annotationType, this.annotationIcon);
            UpdateLayout();
        }

        /// <summary>
        /// Load icon from the AnnotationType value.
        /// </summary>
        private static Icon LoadIconFromKnownAnnotationType(AnnotationType type, AnnotationIcon anicon)
        {
            if (type == AnnotationType.None || anicon == AnnotationIcon.None)
            {
                return null;
            }

            var largeIcon = anicon == AnnotationIcon.Large;
            if (type == AnnotationType.Application)
            {
                IntPtr hicoLarge, hicoSmall;
                if (UnsafeNativeMethods.ExtractIconEx(Application.ExecutablePath, 0, out hicoLarge, out hicoSmall, 1) == 2)
                {
                    var icon = Icon.FromHandle(largeIcon ? hicoLarge : hicoSmall);
                    var iconSize = largeIcon ? SystemInformation.IconSize : SystemInformation.SmallIconSize;
                    icon = new Icon(icon, iconSize);
                    UnsafeNativeMethods.DestroyIcon(hicoLarge);
                    UnsafeNativeMethods.DestroyIcon(hicoSmall);
                    return icon;
                }
                else
                {
                    // the app doesn't have an icon.
                    return LoadIconFromSysIconID(NativeMethods.IDI_APPLICATION, largeIcon);
                }
            }
            else
            {
                int resId = ResourceIdFromKnownAnnotationType(type);
                return LoadIconFromSysIconID(resId, largeIcon);
            }
        }

        /// <summary>
        /// Convert the AnnotationType value to the corresponding Win32 system icon ID.
        /// </summary>
        static int ResourceIdFromKnownAnnotationType(AnnotationType type)
        {
            switch (type)
            {
                case AnnotationType.Error:
                case AnnotationType.Hand:
                    return NativeMethods.IDI_HAND;

                case AnnotationType.Exclamation:
                case AnnotationType.Warning:
                    return NativeMethods.IDI_EXCLAMATION;

                case AnnotationType.Information:
                case AnnotationType.Asterisk:
                    return NativeMethods.IDI_ASTERISK;

                case AnnotationType.Question:
                    return NativeMethods.IDI_QUESTION;

                case AnnotationType.Shield:
                    return NativeMethods.IDI_SHIELD;

                case AnnotationType.WinLogo:
                    return NativeMethods.IDI_WINLOGO;
            }

            throw new ArgumentException("type");
        }

        /// <summary>
        /// Load icon from Win32 icon ID.
        /// </summary>
        private static Icon LoadIconFromSysIconID(int resId, bool largeIcon)
        {
            Icon icon = null;
            IntPtr hico = UnsafeNativeMethods.LoadIcon(IntPtr.Zero, new IntPtr(resId));
            if (hico != IntPtr.Zero)
            {
                var iconSize = largeIcon ? SystemInformation.IconSize : SystemInformation.SmallIconSize;
                IntPtr hicoClone = UnsafeNativeMethods.CopyImage(hico, NativeMethods.IMAGE_ICON, iconSize.Width, iconSize.Height, NativeMethods.LR_COPYFROMRESOURCE);
                if (hicoClone != IntPtr.Zero)
                {
                    icon = Icon.FromHandle(hicoClone);
                    icon = new Icon(icon, iconSize);
                    UnsafeNativeMethods.DestroyIcon(hicoClone);
                }
                UnsafeNativeMethods.DestroyIcon(hico);
            }
            return icon;
        }

        /// <summary>
        /// Convert an AnnotationType value to the corresponding colours.
        /// </summary>
        /// <param name="type">The value of AnnotationType property.</param>
        /// <param name="backColour">The background colour.</param>
        /// <param name="foreColour">The foreground colour.</param>
        private static void ColourFromKnownKnownAnnotationType(AnnotationType type, out Color backColour, out Color foreColour)
        {
            switch (type)
            {
                case AnnotationType.None:
                    backColour = SystemColors.Info;
                    foreColour = SystemColors.InfoText;
                    return;

                case AnnotationType.Application:
                    backColour = SystemColors.Window;
                    foreColour = SystemColors.WindowText;
                    return;

                case AnnotationType.WinLogo:
                    backColour = SystemColors.Window;
                    foreColour = SystemColors.WindowText;
                    return;

                case AnnotationType.Error:
                case AnnotationType.Hand:
                    backColour = Color.MistyRose;
                    foreColour = Color.Black;
                    return;

                case AnnotationType.Exclamation:
                case AnnotationType.Warning:
                    backColour = Color.LemonChiffon;
                    foreColour = Color.Black;
                    return;

                case AnnotationType.Information:
                case AnnotationType.Asterisk:
                    backColour = Color.Honeydew;
                    foreColour = Color.Black;
                    return;

                case AnnotationType.Shield:
                    backColour = Color.PapayaWhip;
                    foreColour = Color.Black;
                    return;

                case AnnotationType.Question:
                    backColour = Color.LightCyan;
                    foreColour = Color.Black;
                    return;
            }

            throw new ArgumentException("type");
        }
    }
}
