using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Cool
{
    public partial class DemoForm : Form
    {
        #region P/Invoke

        private static class UnsafeNativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryW")]
            internal static extern IntPtr LoadLibrary(string path);
            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int DestroyIcon(IntPtr hico);
            [DllImport("comctl32.dll", ExactSpelling = true)]
            internal static extern int LoadIconWithScaleDown(IntPtr hinst, IntPtr name, int cx, int cy, out IntPtr hico);
        }

        #endregion

        public DemoForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // !!FOR DEMONSTRATION PURPOSES ONLY!!
            // add a bunch of icons in a hacky way
            AddListItemsLazy(16, "Computer", 18, "Network", 33, "Recycle Bin", 22, "Control Panel", 17, "Printer", 226, "Camera", 23, "Search", 24, "Help", 160, "Run", 166, "Disk", 48, "Lock", 161, "Warning", 4, "Folder", 1, "File");
            this.panel1.BackgroundImage = Painter.CreateDiagonalGradient(256, Color.White, Color.FromArgb(224, 240, 255));
            this.toolStrip.Renderer = new SimpleToolStripRenderer(Color.NavajoWhite, Color.SandyBrown, Color.FromArgb(255, 192, 80), Color.Chocolate);
        }

        #region "Event Handlers"

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
            this.expiry.Text = value.HasValue ? $"Expires on {value:D}" : "Never expires";
            this.expiry.AnnotationType = value.HasValue ? AnnotationType.Warning : AnnotationType.Information;
        }

        #endregion

        private void AddListItemsLazy(params object[] args)
        {
            var hmod = UnsafeNativeMethods.LoadLibrary("shell32.dll");
            for (var i = 0; i < args.Length; i += 2)
            {
                IntPtr hico;
                if (UnsafeNativeMethods.LoadIconWithScaleDown(hmod, new IntPtr((int)args[i]), 256, 256, out hico) >= 0)
                {
                    using (var icon = Icon.FromHandle(hico))
                    {
                        this.imageList.Images.Add(icon);
                        this.listView.Items.Add((string)args[i + 1], this.imageList.Images.Count - 1);
                    }
                    UnsafeNativeMethods.DestroyIcon(hico);
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
