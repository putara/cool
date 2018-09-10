using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;

namespace Cool
{
    [
        DefaultEvent("LinkClicked"),
        Description("The link area with an icon and a description.")
    ]
    class LinkPanel : Control
    {
        const int MARGINS = 3;

        LinkLabelAlt linkLabel;
        Label descLabel;
        Image image;

        event LinkLabelLinkClickedEventHandler linkClickedEvent;

        public LinkPanel()
        {
            InitialiseComponent();
        }

        [Category(CategoryNames.Appearance)]
        public Color LinkColor
        {
            get { return this.linkLabel.LinkColor; }
            set { this.linkLabel.LinkColor = value; }
        }

        [
            Category(CategoryNames.Appearance),
            DefaultValue("")
        ]
        public string LinkText
        {
            get { return this.linkLabel.Text; }
            set
            {
                this.linkLabel.Text = value;
                UpdateLayout();
            }
        }

        [
            Category(CategoryNames.Appearance),
            DefaultValue("")
        ]
        public string DescriptionText
        {
            get { return this.descLabel.Text; }
            set
            {
                this.descLabel.Text = value;
                UpdateLayout();
            }
        }

        [
            Category(CategoryNames.Behavior),
            DefaultValue(LinkBehavior.HoverUnderline)
        ]
        public LinkBehavior LinkBehavior
        {
            get { return this.linkLabel.LinkBehavior; }
            set { this.linkLabel.LinkBehavior = value; }
        }

        [
            Category(CategoryNames.Appearance),
            DefaultValue(null)
        ]
        public Image Image
        {
            get { return this.image; }
            set
            {
                this.image = value;
                UpdateLayout();
            }
        }

        [
            Browsable(false),
            EditorBrowsable(EditorBrowsableState.Never)
        ]
        public new string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
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

        internal void ResetLinkColor()
        {
            this.linkLabel.ResetLinkColor();
        }

        internal bool ShouldSerializeLinkColor()
        {
            return this.linkLabel.ShouldSerializeLinkColor();
        }

        void InitialiseComponent()
        {
            this.SuspendLayout();

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.linkLabel = new LinkLabelAlt();
            this.linkLabel.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabel.AutoSize = true;
            this.linkLabel.LinkClicked += LinkLabel_LinkClicked;

            this.descLabel = new Label();
            this.descLabel.AutoSize = true;

            this.Controls.Add(this.linkLabel);
            this.Controls.Add(this.descLabel);

            this.ResumeLayout(true);
        }

        protected virtual void OnLinkClicked(LinkLabelLinkClickedEventArgs e)
        {
            if (this.linkClickedEvent != null)
            {
                this.linkClickedEvent(this, e);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateLayout();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            InvokePaintBackground(this, e);

            if (this.image != null)
            {
                var rect = GetDrawingRect();
                var pt = new Point(rect.X, rect.Y + (rect.Height - this.image.Height) / 2);
                e.Graphics.DrawImageUnscaled(this.image, pt);
            }
        }

        void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OnLinkClicked(e);
        }

        Rectangle GetDrawingRect()
        {
            var rect = this.ClientRectangle;
            rect.X += this.Padding.Left;
            rect.Y += this.Padding.Top;
            rect.Width -= this.Padding.Right;
            rect.Height -= this.Padding.Bottom;
            return rect;
        }

        void UpdateLayout()
        {
            this.SuspendLayout();

            var rect = GetDrawingRect();

            if (this.image != null)
            {
                rect.X += this.image.Width + MARGINS;
            }

            int x = rect.X;
            int y = rect.Y;
            if (string.IsNullOrEmpty(this.DescriptionText))
            {
                y += (rect.Height - this.linkLabel.Height - MARGINS * 2) / 2;
            }
            else
            {
                y += (rect.Height - this.linkLabel.Height - this.descLabel.Height - MARGINS * 4) / 2;
                var descPt = new Point(x, y + this.linkLabel.Height + MARGINS * 2);
                this.descLabel.Location = descPt;
            }

            var linkPt = new Point(x, y + MARGINS);
            this.linkLabel.Location = linkPt;

            this.ResumeLayout(true);
            this.Invalidate();
        }
    }
}
