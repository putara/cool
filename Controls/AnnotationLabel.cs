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
                return ShellUtils.ExtractIcon(Application.ExecutablePath, largeIcon ? ShellUtils.IconSize.Large : ShellUtils.IconSize.Small);
            }
            else
            {
                var stockIcon = StockIconIdFromKnownAnnotationType(type);
                return ShellUtils.GetStockIcon(stockIcon, largeIcon ? ShellUtils.IconSize.Large : ShellUtils.IconSize.Small);
            }
        }

        /// <summary>
        /// Convert the AnnotationType value to the corresponding shell stock icon ID.
        /// </summary>
        static ShellUtils.StockIcon StockIconIdFromKnownAnnotationType(AnnotationType type)
        {
            switch (type)
            {
                case AnnotationType.Error:
                case AnnotationType.Hand:
                    return ShellUtils.StockIcon.Error;

                case AnnotationType.Exclamation:
                case AnnotationType.Warning:
                    return ShellUtils.StockIcon.Warning;

                case AnnotationType.Information:
                case AnnotationType.Asterisk:
                    return ShellUtils.StockIcon.Info;

                case AnnotationType.Question:
                    return ShellUtils.StockIcon.Help;

                case AnnotationType.Shield:
                    return ShellUtils.StockIcon.Shield;

                case AnnotationType.WinLogo:
                    // WinLogo is leftover from Win95
                    return ShellUtils.StockIcon.Application;
            }

            throw new ArgumentException("type");
        }

        /// <summary>
        /// Convert an AnnotationType value to the corresponding colours.
        /// </summary>
        /// <param name="type">The value of AnnotationType property.</param>
        /// <param name="backColour">The background colour.</param>
        /// <param name="foreColour">The foreground colour.</param>
        private static void ColourFromKnownKnownAnnotationType(AnnotationType type, out Color backColour, out Color foreColour)
        {
            // TODO: support high contrast
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
