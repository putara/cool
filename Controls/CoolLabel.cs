using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Cool
{
    class CoolLabel : Label
    {
        Color shadowColour = Color.Empty;

        public CoolLabel()
        {
            base.DoubleBuffered = true;
        }

        [Category("Appearance")]
        public Color ShadowColor
        {
            get
            {
                return this.shadowColour;
            }
            set
            {
                this.shadowColour = value;
                this.Invalidate();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void ResetShadowColor()
        {
            this.shadowColour = Color.Empty;
        }

        internal bool ShouldSerializeShadowColor()
        {
            return this.shadowColour.IsEmpty == false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            InvokePaintBackground(this, e);
            var textFormat = TextFormatFlags.SingleLine;
            var stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;

            CalculateFormatFlags(ref textFormat, ref stringFormat);

            var rect = this.ClientRectangle;
            rect.X += this.Padding.Left;
            rect.Y += this.Padding.Top;
            rect.Width -= this.Padding.Right;
            rect.Height -= this.Padding.Bottom;

            var font = this.Font;
            var shadowRect1 = rect;
            shadowRect1.Offset(1, 1);
            var shadowRect2 = rect;
            shadowRect2.Offset(2, 2);

            if (this.UseCompatibleTextRendering)
            {
                using (var foreBrush = new SolidBrush(this.ForeColor))
                {
                    if (this.shadowColour.IsEmpty == false)
                    {
                        e.Graphics.DrawString(this.Text, font, foreBrush, shadowRect2, stringFormat);
                        using (var shadowBrush = new SolidBrush(this.ShadowColor))
                        {
                            e.Graphics.DrawString(this.Text, font, shadowBrush, shadowRect1, stringFormat);
                        }
                    }
                    e.Graphics.DrawString(this.Text, font, foreBrush, rect, stringFormat);
                }
            }
            else
            {
                if (this.shadowColour.IsEmpty == false)
                {
                    TextRenderer.DrawText(e.Graphics, this.Text, font, shadowRect2, this.ForeColor, textFormat);
                    TextRenderer.DrawText(e.Graphics, this.Text, font, shadowRect1, this.ShadowColor, textFormat);
                }
                TextRenderer.DrawText(e.Graphics, this.Text, font, rect, this.ForeColor, textFormat);
            }
        }

        void CalculateFormatFlags(ref TextFormatFlags textFormat, ref StringFormat stringFormat)
        {
            if (this.RightToLeft == RightToLeft.Yes)
            {
                textFormat |= TextFormatFlags.RightToLeft;
                stringFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
            }

            if ((this.TextAlign & (ContentAlignment.TopLeft | ContentAlignment.MiddleLeft | ContentAlignment.BottomLeft)) != 0)
            {
                textFormat |= TextFormatFlags.Left;
                stringFormat.Alignment = StringAlignment.Near;
            }
            else if ((this.TextAlign & (ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter)) != 0)
            {
                textFormat |= TextFormatFlags.HorizontalCenter;
                stringFormat.Alignment = StringAlignment.Center;
            }
            else if ((this.TextAlign & (ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight)) != 0)
            {
                textFormat |= TextFormatFlags.Right;
                stringFormat.Alignment = StringAlignment.Far;
            }

            if ((this.TextAlign & (ContentAlignment.TopLeft | ContentAlignment.TopCenter | ContentAlignment.TopRight)) != 0)
            {
                textFormat |= TextFormatFlags.Top;
                stringFormat.LineAlignment = StringAlignment.Near;
            }
            else if ((this.TextAlign & (ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight)) != 0)
            {
                textFormat |= TextFormatFlags.VerticalCenter;
                stringFormat.LineAlignment = StringAlignment.Center;
            }
            else if ((this.TextAlign & (ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight)) != 0)
            {
                textFormat |= TextFormatFlags.Bottom;
                stringFormat.LineAlignment = StringAlignment.Far;
            }
        }
    }
}
