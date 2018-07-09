namespace Cool
{
    partial class DemoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Cool.ThemedSplitter splitterVert;
            Cool.ThemedSplitter splitterHorz;
            System.Windows.Forms.ToolStripStatusLabel ready;
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.exitMenu = new System.Windows.Forms.ToolStripButton();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.listView = new Cool.CoolListView();
            this.column1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new Cool.DoubleBufferedPanel();
            this.panel2 = new Cool.DoubleBufferedPanel();
            this.expiry = new Cool.AnnotationLabel();
            this.datePicker = new Cool.DateTimePickerEx();
            this.linkPanel = new Cool.LinkPanel();
            this.status = new Cool.StatusStripFix();
            this.caption = new Cool.CaptionBar();
            splitterVert = new Cool.ThemedSplitter();
            splitterHorz = new Cool.ThemedSplitter();
            ready = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.status.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.BackColor = System.Drawing.Color.RoyalBlue;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitMenu});
            this.toolStrip.Location = new System.Drawing.Point(0, 39);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip.Size = new System.Drawing.Size(684, 39);
            this.toolStrip.TabIndex = 1;
            // 
            // exitMenu
            // 
            this.exitMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.exitMenu.Image = global::Cool.Properties.Resources.exit;
            this.exitMenu.Name = "exitMenu";
            this.exitMenu.Size = new System.Drawing.Size(36, 36);
            this.exitMenu.Text = "Exit";
            this.exitMenu.Click += new System.EventHandler(this.exitMenu_Click);
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new System.Drawing.Size(64, 64);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // listView
            // 
            this.listView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.column1});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.LargeImageList = this.imageList;
            this.listView.Location = new System.Drawing.Point(0, 78);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(441, 366);
            this.listView.TabIndex = 2;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // column1
            // 
            this.column1.Text = "";
            // 
            // splitterVert
            // 
            splitterVert.Dock = System.Windows.Forms.DockStyle.Right;
            splitterVert.Location = new System.Drawing.Point(441, 78);
            splitterVert.Name = "splitterVert";
            splitterVert.Size = new System.Drawing.Size(3, 366);
            splitterVert.TabIndex = 3;
            splitterVert.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(splitterHorz);
            this.panel1.Controls.Add(this.linkPanel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(444, 78);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(240, 366);
            this.panel1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.expiry);
            this.panel2.Controls.Add(this.datePicker);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 83);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(8);
            this.panel2.Size = new System.Drawing.Size(240, 283);
            this.panel2.TabIndex = 2;
            // 
            // expiry
            // 
            this.expiry.BackColor = System.Drawing.Color.Transparent;
            this.expiry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.expiry.Location = new System.Drawing.Point(8, 31);
            this.expiry.Name = "expiry";
            this.expiry.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.expiry.Size = new System.Drawing.Size(224, 244);
            this.expiry.TabIndex = 1;
            this.expiry.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.expiry.Visible = false;
            // 
            // datePicker
            // 
            this.datePicker.Checked = false;
            this.datePicker.Dock = System.Windows.Forms.DockStyle.Top;
            this.datePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.datePicker.Location = new System.Drawing.Point(8, 8);
            this.datePicker.Name = "datePicker";
            this.datePicker.Size = new System.Drawing.Size(224, 23);
            this.datePicker.TabIndex = 0;
            this.datePicker.Visible = false;
            this.datePicker.ValueChanged += new System.EventHandler(this.datePicker_ValueChanged);
            // 
            // splitterHorz
            // 
            splitterHorz.Dock = System.Windows.Forms.DockStyle.Top;
            splitterHorz.Location = new System.Drawing.Point(0, 80);
            splitterHorz.Name = "splitterHorz";
            splitterHorz.Size = new System.Drawing.Size(240, 3);
            splitterHorz.TabIndex = 1;
            splitterHorz.TabStop = false;
            // 
            // linkPanel
            // 
            this.linkPanel.BackColor = System.Drawing.Color.Transparent;
            this.linkPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.linkPanel.Location = new System.Drawing.Point(0, 0);
            this.linkPanel.Name = "linkPanel";
            this.linkPanel.Size = new System.Drawing.Size(240, 80);
            this.linkPanel.TabIndex = 0;
            // 
            // status
            // 
            this.status.BackColor = System.Drawing.Color.CornflowerBlue;
            this.status.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            ready});
            this.status.Location = new System.Drawing.Point(0, 444);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(684, 22);
            this.status.TabIndex = 5;
            // 
            // ready
            // 
            ready.ForeColor = System.Drawing.Color.White;
            ready.Name = "ready";
            ready.Size = new System.Drawing.Size(39, 17);
            ready.Text = "Ready";
            // 
            // caption
            // 
            this.caption.BackColor = System.Drawing.Color.RoyalBlue;
            this.caption.Font = new System.Drawing.Font("Trebuchet MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.caption.Location = new System.Drawing.Point(0, 0);
            this.caption.Name = "caption";
            this.caption.Size = new System.Drawing.Size(684, 39);
            this.caption.TabIndex = 0;
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(684, 466);
            this.Controls.Add(this.listView);
            this.Controls.Add(splitterVert);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.status);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.caption);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "DemoForm";
            this.Text = "Custom Controls Demo";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.status.ResumeLayout(false);
            this.status.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CoolListView listView;
        private CaptionBar caption;
        private DoubleBufferedPanel panel1;
        private System.Windows.Forms.ToolStrip toolStrip;
        private StatusStripFix status;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ToolStripButton exitMenu;
        private System.Windows.Forms.ColumnHeader column1;
        private LinkPanel linkPanel;
        private DateTimePickerEx datePicker;
        private AnnotationLabel expiry;
        private DoubleBufferedPanel panel2;
    }
}

