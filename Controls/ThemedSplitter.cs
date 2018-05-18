using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;

namespace Cool
{
    class ThemedSplitter : Splitter
    {
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private new Color BackColor
        {
            get { return Color.Empty; }
            set { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var rect = this.ClientRectangle;
            e.Graphics.FillRectangle(SystemBrushes.Window, rect);
            if (this.Dock == DockStyle.Left || this.Dock == DockStyle.Right)
            {
                rect.X += rect.Width / 2;
                rect.Width = 1;
            }
            else if (this.Dock == DockStyle.Top || this.Dock == DockStyle.Bottom)
            {
                rect.Y += rect.Height / 2;
                rect.Height = 1;
            }
            else
            {
                return;
            }
            var barColour = ColourUtils.Blend(SystemColors.Window, SystemColors.ButtonFace, .5f);
            using (var br = new SolidBrush(barColour))
            {
                e.Graphics.FillRectangle(br, rect);
            }
        }
    }
}
