using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cool
{
    public class SimpleToolStripRenderer : ToolStripSystemRenderer
    {
        Color buttonSelectedBackColour;
        Color buttonSelectedBorderColour;
        Color buttonPressedBackColour;
        Color buttonPressedBorderColour;

        public SimpleToolStripRenderer(Color selectedBackColour, Color selectedBorderColour, Color pressedBackColour, Color pressedBorderColour)
        {
            this.buttonSelectedBackColour = selectedBackColour;
            this.buttonSelectedBorderColour = selectedBorderColour;
            this.buttonPressedBackColour = pressedBackColour;
            this.buttonPressedBorderColour = pressedBorderColour;
        }

        static bool IsItemChecked(ToolStripItem item)
        {
            var btn = item as ToolStripButton;
            return btn != null && btn.Checked;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
        }

        protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
        {
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            OnRenderButtonBackground(e);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            var bgColour = Color.Empty;
            var borderColour = Color.Empty;
            var itemRect = new Rectangle(0, 0, e.Item.Width - 1, e.Item.Height - 1);

            if (e.Item.Pressed)
            {
                bgColour = this.buttonPressedBackColour;
                borderColour = this.buttonPressedBorderColour;
            }
            else if (e.Item.Selected)
            {
                bgColour = this.buttonSelectedBackColour;
                borderColour = this.buttonSelectedBorderColour;
            }
            else if (IsItemChecked(e.Item))
            {
                bgColour = ColourUtils.Blend(e.ToolStrip.BackColor, this.buttonSelectedBackColour, .125f);
                borderColour = this.buttonSelectedBorderColour;
            }

            if (bgColour.IsEmpty == false)
            {
                using (var brush = new SolidBrush(bgColour))
                {
                    using (var pen = new Pen(borderColour))
                    {
                        e.Graphics.FillRectangle(brush, itemRect);
                        e.Graphics.DrawRectangle(pen, itemRect);
                    }
                }
            }
        }
    }
}
