using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Cool
{
    public partial class DemoForm : Form
    {
        public DemoForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // add stock icons
            this.PopulateListView();
            this.panel1.BackgroundImage = Painter.CreateDiagonalGradient(256, Color.White, Color.FromArgb(224, 240, 255));
            this.toolStrip.Renderer = new SimpleToolStripRenderer(Color.Coral, Color.Tomato, Color.Firebrick, Color.DarkRed);
        }

        #region Event Handlers

        private void exitMenu_Click(object sender, EventArgs e)
        {
            var dlg = new TaskDialog();
            dlg.Buttons.Add(new TaskDialogButton(TaskDialogResult.OK, "Exit application right now\nI'm bored.", true));
            dlg.Buttons.Add(new TaskDialogButton(TaskDialogResult.Cancel, "Please don't exit\nI'm still into it."));
            dlg.Buttons.Add(new TaskDialogButton(TaskDialogResult.UserButton1, "Go to the website\nI want details."));
            dlg.AllowCancellation = true;
            dlg.Appearance = TaskDialogAppearance.CommandLinks;
            dlg.CommonButtons = TaskDialogCommonButtons.Cancel;
            dlg.ContentText = "Choose one of them.";
            dlg.ExpandedInformation = "Kia ora";
            dlg.FooterAppearance = TaskDialogFooterAppearance.Expandable;
            dlg.FooterIcon = TaskDialogIcon.Information;
            dlg.FooterText = "Copyright © 2018";
            dlg.MainIcon = TaskDialogIcon.Warning;
            dlg.MainInstruction = "What do you want?";
            dlg.SizeToContent = true;
            dlg.WindowTitle = "Task Dialog Demo";
            var result = dlg.ShowDialog(this);
            if (result == TaskDialogResult.OK)
            {
                this.Close();
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = GetSelectedItem();
            this.panel2.SuspendLayout();
            if (item != null)
            {
                this.linkPanel.LinkText = item.Text;
                this.linkPanel.DescriptionText = "Open " + item.Text;
                this.linkPanel.Image = item.ImageList.Images[item.ImageIndex];
                this.datePicker.Value = item.Tag as DateTime?;
                // explicitly call event handler
                this.datePicker_ValueChanged(this.datePicker, EventArgs.Empty);
                this.datePicker.Show();
                this.expiry.Show();
            }
            else
            {
                this.linkPanel.LinkText = null;
                this.linkPanel.DescriptionText = null;
                this.linkPanel.Image = null;
                this.datePicker.Hide();
                this.expiry.Hide();
            }
            this.panel2.ResumeLayout(true);
        }

        private void datePicker_ValueChanged(object sender, EventArgs e)
        {
            var value = this.datePicker.Value;
            var item = GetSelectedItem();
            if (item != null)
            {
                item.Tag = value;
            }
            if (value.HasValue)
            {
                // valid until 11:59:59 pm
                if (DateTime.Now.CompareTo(value.Value.AddSeconds(86399)) > 0)
                {
                    this.expiry.Text = "Already expired";
                    this.expiry.AnnotationType = AnnotationType.Error;
                }
                else
                {
                    this.expiry.Text = $"Expires on {value:D}";
                    this.expiry.AnnotationType = AnnotationType.Warning;
                }
            }
            else
            {
                this.expiry.Text = "Never expires";
                this.expiry.AnnotationType = AnnotationType.Information;
            }
        }

        #endregion

        private void PopulateListView()
        {
            foreach (ShellUtils.StockIcon stockIcon in Enum.GetValues(typeof(ShellUtils.StockIcon)))
            {
                using (var icon = ShellUtils.GetStockIcon(stockIcon, ShellUtils.IconSize.ShellSize))
                {
                    if (this.imageList.Images.Count == 0)
                    {
                        this.imageList.ImageSize = icon.Size;
                    }
                    this.imageList.Images.Add(icon);
                    this.listView.Items.Add(stockIcon.ToString(), this.imageList.Images.Count - 1);
                }
            }
        }

        private ListViewItem GetSelectedItem()
        {
            var item = this.listView.FocusedItem;
            if (item == null || item.Selected == false)
            {
                var items = this.listView.SelectedItems;
                item = items != null && items.Count > 0 ? items[0] : null;
            }
            return item;
        }
    }
}
