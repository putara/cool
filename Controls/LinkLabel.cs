using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using Debug = System.Diagnostics.Debug;

namespace Cool
{
    [
        DefaultEvent("LinkClicked"),
        Description("Yet another LinkLabel, aka reinventing the wheel.")
    ]
    public class LinkLabelAlt : Label
    {
        Cursor cursor = null;
        Color foreColour = Color.Empty;
        Color activeLinkColour = Color.Empty;
        Color linkColour = Color.Empty;
        LinkBehavior linkBehaviour = LinkBehavior.SystemDefault;
        bool drawShadow = false;
        bool mouseIn = false;
        bool drawFocusRect = false;

        event LinkLabelLinkClickedEventHandler linkClickedEvent;

        public LinkLabelAlt()
        {
            base.Cursor = GraphicUtils.Hand;
            this.drawFocusRect = SystemInformation.IsKeyboardPreferred;
            base.DoubleBuffered = true;
            SetStyle(ControlStyles.Selectable, true);
        }

        [AmbientValue(null)]
        public new Cursor Cursor
        {
            get
            {
                if (this.cursor == null)
                {
                    return DefaultCursor;
                }
                return this.cursor;
            }
            set
            {
                this.cursor = value;
            }
        }

        [Category(CategoryNames.Appearance)]
        public new Color ForeColor
        {
            get
            {
                if (this.foreColour.IsEmpty)
                {
                    Control parent = this.Parent;
                    if (parent != null)
                    {
                        return parent.ForeColor;
                    }
                    return DefaultForeColor;
                }
                return this.foreColour;
            }
            set
            {
                this.foreColour = value;
            }
        }

        [Category(CategoryNames.Appearance)]
        public Color ActiveLinkColor
        {
            get
            {
                if (this.activeLinkColour.IsEmpty)
                {
                    if (this.linkColour.IsEmpty)
                    {
                        return SystemColors.Highlight;
                    }
                    return this.linkColour;
                }
                return this.activeLinkColour;
            }
            set
            {
                this.activeLinkColour = value;
            }
        }

        [Category(CategoryNames.Appearance)]
        public Color LinkColor
        {
            get
            {
                if (this.linkColour.IsEmpty)
                {
                    return SystemColors.HotTrack;
                }
                return this.linkColour;
            }
            set
            {
                this.linkColour = value;
                base.ForeColor = value;
            }
        }

        [
            Category(CategoryNames.Behavior),
            DefaultValue(LinkBehavior.SystemDefault)
        ]
        public LinkBehavior LinkBehavior
        {
            get
            {
                return this.linkBehaviour;
            }
            set
            {
                this.linkBehaviour = value;
                this.Invalidate();
            }
        }

        [
            Browsable(true),
            EditorBrowsable(EditorBrowsableState.Always),
            DefaultValue(true)
        ]
        public new bool TabStop
        {
            get
            {
                return base.TabStop;
            }
            set
            {
                base.TabStop = value;
            }
        }

        [
            Category(CategoryNames.Appearance),
            DefaultValue(false)
        ]
        public bool DrawShadow
        {
            get { return this.drawShadow; }
            set
            {
                if (this.drawShadow != value)
                {
                    this.drawShadow = value;
                    this.Invalidate();
                }
            }
        }

        [Category(CategoryNames.Action)]
        public event LinkLabelLinkClickedEventHandler LinkClicked
        {
            add
            {
                this.linkClickedEvent += value;
            }
            remove
            {
                this.linkClickedEvent -= value;
            }
        }

        [
            Browsable(true),
            EditorBrowsable(EditorBrowsableState.Always)
        ]
        public new event EventHandler TabStopChanged
        {
            add
            {
                base.TabStopChanged += value;
            }
            remove
            {
                base.TabStopChanged -= value;
            }
        }

        [
            DefaultValue(AccessibleRole.Link),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public new AccessibleRole AccessibleRole
        {
            get
            {
                return AccessibleRole.Link;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetCursor()
        {
            this.cursor = null;
        }

        internal bool ShouldSerializeCursor()
        {
            return this.cursor != null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ResetForeColor()
        {
            this.foreColour = Color.Empty;
        }

        internal bool ShouldSerializeForeColor()
        {
            return this.foreColour.IsEmpty == false;
        }

        internal void ResetActiveLinkColor()
        {
            this.activeLinkColour = Color.Empty;
        }

        internal bool ShouldSerializeActiveLinkColor()
        {
            return this.activeLinkColour.IsEmpty == false;
        }

        internal void ResetLinkColor()
        {
            this.linkColour = Color.Empty;
            this.Invalidate();
        }

        internal bool ShouldSerializeLinkColor()
        {
            return this.linkColour.IsEmpty == false;
        }

        protected virtual void OnLinkClicked(LinkLabelLinkClickedEventArgs e)
        {
            if (this.linkClickedEvent != null)
            {
                this.linkClickedEvent(this, e);
            }
        }

        void PerformClick(MouseButtons button)
        {
            var link = new LinkLabel.Link(0, this.Text.Length);
            OnLinkClicked(new LinkLabelLinkClickedEventArgs(link, button));
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if ((keyData & Keys.KeyCode) == Keys.Enter)
            {
                if (this.Focused)
                {
                    PerformClick(MouseButtons.None);
                    return true;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter && this.Focused)
            {
                PerformClick(MouseButtons.None);
            }
        }

        protected override void OnChangeUICues(UICuesEventArgs e)
        {
            base.OnChangeUICues(e);
            //Debug.Print("OnChangeUICues: {0} {1}", e.ChangeFocus, e.ShowFocus);

            if (e.ChangeFocus)
            {
                this.drawFocusRect = e.ShowFocus;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left)
            {
                PerformClick(e.Button);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this.mouseIn == false)
            {
                this.mouseIn = true;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (this.mouseIn)
            {
                this.mouseIn = false;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
//          base.OnPaint(e);

            InvokePaintBackground(this, e);
            var textFormat = TextFormatFlags.SingleLine;
            var stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;

            CalculateFormatFlags(ref textFormat, ref stringFormat);

            var style = FontStyle.Regular;
            var active = this.mouseIn || this.Focused;
            var colour = active ? this.ActiveLinkColor : this.LinkColor;

            switch (this.linkBehaviour)
            {
                case LinkBehavior.SystemDefault:
                case LinkBehavior.AlwaysUnderline:
                    style |= FontStyle.Underline;
                    break;

                case LinkBehavior.HoverUnderline:
                    if (active)
                    {
                        style |= FontStyle.Underline;
                    }
                    break;

                case LinkBehavior.NeverUnderline:
                    break;
            }

            using (var font = new Font(this.Font, style))
            {
                var rect = this.ClientRectangle;
                rect.X += this.Padding.Left;
                rect.Y += this.Padding.Top;
                rect.Width -= this.Padding.Right;
                rect.Height -= this.Padding.Bottom;

                var shadowRect = rect;
                var shadowColour = colour;
                if (this.drawShadow)
                {
                    shadowRect.Offset(1, 1);
                    shadowColour = CalculateShadowColour(colour);
                }

                if (this.UseCompatibleTextRendering)
                {
                    if (this.drawShadow)
                    {
                        using (var brush = new SolidBrush(shadowColour))
                        {
                            e.Graphics.DrawString(this.Text, font, brush, shadowRect, stringFormat);
                        }
                    }
                    using (var brush = new SolidBrush(colour))
                    {
                        e.Graphics.DrawString(this.Text, font, brush, rect, stringFormat);
                    }
                }
                else
                {
                    if (this.drawShadow)
                    {
                        TextRenderer.DrawText(e.Graphics, this.Text, font, shadowRect, shadowColour, textFormat);
                    }
                    TextRenderer.DrawText(e.Graphics, this.Text, font, rect, colour, textFormat);
                }

                if (this.drawFocusRect && this.Focused)
                {
                    ControlPaint.DrawFocusRectangle(e.Graphics, rect);
                }
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

        // copied from CaptionBar.cs.
        public static Color BlendColours(Color baseColour, Color overlayColour, float alpha)
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

        Color CalculateShadowColour(Color colour)
        {
            Color backColour = this.BackColor;
            if (backColour.IsEmpty)
            {
                backColour = this.Parent.BackColor;
            }
            colour = colour.GetBrightness() > .5f ? Color.Black : Color.White;
            return BlendColours(colour, backColour, .5f);
        }

        //static Color CalculateShadowColour(Color colour)
        //{
        //  float hue = colour.GetHue();
        //  float saturation = colour.GetSaturation();
        //  float brightness = colour.GetBrightness();
        //  if (brightness < .5f)
        //  {
        //      brightness = Math.Min(1, brightness + .75f);
        //  }
        //  else
        //  {
        //      brightness = Math.Max(0, brightness - .75f);
        //  }
        //  return FromHSB(hue, saturation, brightness);
        //}

        //// https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSL
        //static Color FromHSB(float hue, float saturation, float brightness)
        //{
        //  float c = brightness * saturation;
        //  float h = hue * 6f;
        //  float x = c * (1 - Math.Abs(h % 2 - 1));
        //  float r, g, b;

        //  switch ((int)Math.Floor(h))
        //  {
        //      case 0:
        //          r = c; g = x; b = 0; break;
        //      case 1:
        //          r = x; g = c; b = 0; break;
        //      case 2:
        //          r = 0; g = c; b = x; break;
        //      case 3:
        //          r = 0; g = x; b = c; break;
        //      case 4:
        //          r = x; g = 0; b = c; break;
        //      case 5:
        //      default:
        //          r = c; g = 0; b = x; break;
        //  }

        //  float m = brightness - c;
        //  return Color.FromArgb(
        //          (int)Math.Floor((r + m) * 255),
        //          (int)Math.Floor((g + m) * 255),
        //          (int)Math.Floor((b + m) * 255));
        //}
    }
}
