using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Debug = System.Diagnostics.Debug;

namespace Cool
{
    /// <summary>
    /// Provide a title bar.
    /// No comment!!
    /// </summary>
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    class CaptionBar : Control
    {
        public delegate void BaseWndProc(ref Message m);

        const int BTN_CLOSE = 0;
        const int BTN_RESTORE = 1;
        const int BTN_MAXIMISE = 2;
        const int BTN_MINIMISE = 3;

        //IContainer components;
        ParentWindowListener parentListener;
        CaptionLabel titleLabel;
        CaptionToolStrip captionStrip;
        ToolStripButton[] captionBtns = new ToolStripButton[] { };

        bool restoredSizeSaved;
        Size restoredWindowSize;

        Color foreColour = Color.Black;

        #region P/Invoke

        static class NativeMethods
        {
            internal const int WM_SIZE = 0x0005;
            internal const int WM_ACTIVATE = 0x0006;
            internal const int WM_PAINT = 0x000F;
            internal const int WM_ERASEBKGND = 0x0014;
            internal const int WM_MOUSEACTIVATE = 0x0021;
            internal const int WM_WINDOWPOSCHANGED = 0x0047;
            internal const int WM_NCCALCSIZE = 0x0083;
            internal const int WM_NCHITTEST = 0x0084;
            internal const int WM_NCRBUTTONUP = 0x00A5;
            internal const int WM_DWMCOMPOSITIONCHANGED = 0x031E;
            //internal const int WM_DWMNCRENDERINGCHANGED = 0x031F;
            //internal const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
            //internal const int WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321;

            internal const int HTTRANSPARENT = -1;
            internal const int HTNOWHERE = 0;
            internal const int HTCAPTION = 2;
            internal const int HTSYSMENU = 3;
            internal const int HTLEFT = 10;
            internal const int HTRIGHT = 11;
            internal const int HTTOP = 12;
            internal const int HTTOPLEFT = 13;
            internal const int HTTOPRIGHT = 14;
            internal const int HTBOTTOM = 15;
            internal const int HTBOTTOMLEFT = 16;
            internal const int HTBOTTOMRIGHT = 17;

            internal const uint WS_CAPTION = 0x00C00000;

            internal const int GWL_STYLE = -16;
            internal const int GWL_EXSTYLE = -20;

            internal const int WA_INACTIVE = 0;

            internal const int MA_ACTIVATE = 1;

            internal const uint CWP_SKIPINVISIBLEDISABLEDTRANSPARENT = 0x0007;

            internal const uint SWP_NOSIZEMOVEZORDERREDRAWACTIVATE = 0x1 | 0x2 | 0x4 | 0x8 | 0x10;
            internal const uint SWP_FRAMECHANGED = 0x0020;

            internal const int SW_SHOWNORMAL = 1;
            internal const int SW_SHOWMINIMIZED = 2;
            internal const int SW_SHOWMAXIMIZED = 3;

            [StructLayout(LayoutKind.Sequential)]
            internal struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;

                public RECT(int left, int top, int right, int bottom)
                {
                    this.left = left;
                    this.top = top;
                    this.right = right;
                    this.bottom = bottom;
                }

                public RECT(Rectangle r)
                {
                    this.left = r.Left;
                    this.top = r.Top;
                    this.right = r.Right;
                    this.bottom = r.Bottom;
                }

                public Rectangle ToRectangle()
                {
                    return new Rectangle(this.left, this.top, this.right - this.left, this.bottom - this.top);
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WINDOWPLACEMENT
            {
                public int length;
                public uint flags;
                public int showCmd;
                public POINT ptMinPosition;
                public POINT ptMaxPosition;
                public RECT rcNormalPosition;
            };

            [StructLayout(LayoutKind.Sequential)]
            internal struct MARGINS
            {
                public int cxLeftWidth;
                public int cxRightWidth;
                public int cyTopHeight;
                public int cyBottomHeight;

                public MARGINS(int left, int top, int right, int bottom)
                {
                    this.cxLeftWidth = left;
                    this.cxRightWidth = right;
                    this.cyTopHeight = top;
                    this.cyBottomHeight = bottom;
                }

                public MARGINS(int all)
                {
                    this.cxLeftWidth = all;
                    this.cxRightWidth = all;
                    this.cyTopHeight = all;
                    this.cyBottomHeight = all;
                }
            }

            //[StructLayout(LayoutKind.Sequential)]
            //internal struct NCCALCSIZE_PARAMS
            //{
            //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            //    public RECT[] rgrc;
            //    public IntPtr lppos;
            //}
        }

        static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", EntryPoint = "PostMessageW")]
            internal static extern int PostMessage(IntPtr hwnd, int Msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int GetWindowRect(IntPtr hwnd, ref NativeMethods.RECT lpRect);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int GetClientRect(IntPtr hwnd, ref NativeMethods.RECT lpRect);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int AdjustWindowRectEx(ref NativeMethods.RECT lpRect, uint dwStyle, int bMenu, uint dwExStyle);

            [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
            internal static extern uint GetWindowLong(IntPtr hwnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
            internal static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int cx, int cy, uint flags);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int ValidateRect(IntPtr hwnd, ref NativeMethods.RECT lpRect);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int InvalidateRect(IntPtr hwnd, ref NativeMethods.RECT lpRect, int bErase);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int GetMessagePos();

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern IntPtr ChildWindowFromPointEx(IntPtr hwnd, int x, int y, uint flags);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int GetWindowPlacement(IntPtr hwnd, ref NativeMethods.WINDOWPLACEMENT lpwndpl);

            [DllImport("dwmapi.dll", ExactSpelling = true)]
            internal static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref NativeMethods.MARGINS margins);

            [DllImport("dwmapi.dll", ExactSpelling = true)]
            internal static extern int DwmDefWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref IntPtr plResult);
        }

        #endregion

        #region Inner classes

        class CaptionLabel : Label
        {
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == NativeMethods.WM_NCHITTEST)
                {
                    if (DesignMode == false)
                    {
                        m.Result = (IntPtr)NativeMethods.HTTRANSPARENT;
                        return;
                    }
                }
                base.WndProc(ref m);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                var text = this.Text;
                var font = this.Font;
                var rect = this.Parent.ClientRectangle;
                var colour = this.ForeColor;
                var flags = TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                //var shadowRect = rect;
                //var shadowColour = IsDarkColour(colour) ? Color.White : Color.Black;
                //shadowRect.Offset(1, 1);
                //shadowColour = BlendColours(this.Parent.BackColor, shadowColour, .5f);
                //TextRenderer.DrawText(g, text, font, shadowRect, shadowColour, flags);
                TextRenderer.DrawText(g, text, font, rect, colour, flags);
            }
        }

        class CaptionToolStrip : ToolStrip
        {
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == NativeMethods.WM_MOUSEACTIVATE)
                {
                    if (DesignMode == false)
                    {
                        // the ToolStrip intercepts WM_MOUSEACTIVATE and returns MA_ACTIVATEANDEAT when the mouse cursor is in the control itself.
                        // the following code prevents this annoying behaviour.
                        // we need a caption button to get activated when a user click it.
                        Point pt = this.PointToClient(new Point(UnsafeNativeMethods.GetMessagePos()));
                        IntPtr handle = UnsafeNativeMethods.ChildWindowFromPointEx(this.Handle, pt.X, pt.Y, NativeMethods.CWP_SKIPINVISIBLEDISABLEDTRANSPARENT);
                        if (handle == this.Handle)
                        {
                            m.Result = new IntPtr(NativeMethods.MA_ACTIVATE);
                            return;
                        }
                        else
                        {
                            // don't return; make ToolStrip happy.
                        }
                    }
                }
                base.WndProc(ref m);
            }
        }

        class Renderer : ToolStripRenderer
        {
            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                Color colour;
                if (e.Item.Pressed)
                {
                    colour = ColourUtils.Blend(e.Item.BackColor, Color.Black, .6f);
                }
                else if (e.Item.Selected)
                {
                    colour = e.Item.BackColor;
                }
                else
                {
                    return;
                }
                using (var br = new SolidBrush(colour))
                {
                    e.Graphics.FillRectangle(br, new Rectangle(Point.Empty, e.Item.Size));
                }
            }
        }

        // https://msdn.microsoft.com/en-us/library/system.windows.forms.nativewindow.aspx
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        class ParentWindowListener : NativeWindow
        {
            WeakReference<CaptionBar> captionBar;
            Form parentForm;
            bool zombie = false;

            public ParentWindowListener(CaptionBar captionBar, Form parentForm)
            {
                this.captionBar = new WeakReference<CaptionBar>(captionBar);
                this.parentForm = parentForm;
                parentForm.HandleCreated += Parent_HandleCreated;
                parentForm.HandleDestroyed += Parent_HandleDestroyed;

                if (parentForm.Handle != null)
                {
                    Parent_HandleCreated(parentForm, EventArgs.Empty);
                    // SetWindowPos will fire WM_NCCALCSIZE so we can recalculate the size of the non-client area.
                    UnsafeNativeMethods.SetWindowPos(parentForm.Handle, IntPtr.Zero, 0, 0, 0, 0, NativeMethods.SWP_NOSIZEMOVEZORDERREDRAWACTIVATE | NativeMethods.SWP_FRAMECHANGED);
                }
            }

            void Parent_HandleCreated(object sender, EventArgs e)
            {
                Debug.Print("ParentWindowListener: AssignHandle(0x{0:X})", ((Form)sender).Handle.ToInt32());
                base.AssignHandle(((Form)sender).Handle);
            }

            void Parent_HandleDestroyed(object sender, EventArgs e)
            {
                Debug.Print("ParentWindowListener: ReleaseHandle(0x{0:X})", ((Form)sender).Handle.ToInt32());
                base.ReleaseHandle();
            }

            [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
            protected override void WndProc(ref Message m)
            {
                CaptionBar that;
                if (this.captionBar.TryGetTarget(out that) == false)
                {
                    if (this.zombie == false)
                    {
                        this.zombie = true;
                        Debug.Fail("The CaptinBar has passed away.");
                    }
                    base.WndProc(ref m);
                    return;
                }

                IntPtr result = IntPtr.Zero;
                bool handled = (UnsafeNativeMethods.DwmDefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam, ref result) != 0);
                if (handled)
                {
                    m.Result = result;
                }

                switch (m.Msg)
                {
                    case NativeMethods.WM_SIZE:
                        handled = that.OnParentWmSize(ref m, base.WndProc);
                        break;

                    case NativeMethods.WM_ACTIVATE:
                        handled = that.OnParentWmActivate(ref m, base.WndProc);
                        break;

                    case NativeMethods.WM_PAINT:
                        handled = that.OnParentWmPaint(ref m, base.WndProc);
                        break;

                    case NativeMethods.WM_ERASEBKGND:
                        handled = that.OnParentWmEraseBackground(ref m, base.WndProc);
                        break;

                    case NativeMethods.WM_WINDOWPOSCHANGED:
                        handled = that.OnParentWmWindowPosChanged(ref m, base.WndProc);
                        break;

                    case NativeMethods.WM_NCCALCSIZE:
                        handled = that.OnParentWmNcCalcSize(ref m, base.WndProc);
                        break;

                    case NativeMethods.WM_NCHITTEST:
                        handled = that.OnParentWmNcHitTest(ref m, base.WndProc);
                        break;

                    case NativeMethods.WM_NCRBUTTONUP:
                        handled = that.OnParentWmNcRButtonUp(ref m, base.WndProc);
                        break;

                    case NativeMethods.WM_DWMCOMPOSITIONCHANGED:
                        handled = that.OnParentWmDwmCompositionChanged(ref m, base.WndProc);
                        break;
                }

                if (handled == false)
                {
                    base.WndProc(ref m);
                }
            }
        }

        #endregion

        public CaptionBar()
        {
            InitialiseComponent();
            this.Dock = DockStyle.Top;
            this.SetStyle(ControlStyles.Selectable, false);
        }

        #region Properties

        [DefaultValue(DockStyle.Top)]
        public new DockStyle Dock
        {
            get
            {
                return base.Dock;
            }
            set
            {
                base.Dock = value;
            }
        }

        [
            Browsable(false),
            EditorBrowsable(EditorBrowsableState.Never),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public new Color ForeColor
        {
            get
            {
                return this.foreColour;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        private Form ParentForm
        {
            get
            {
                return this.FindForm();
            }
        }

        #endregion

        void InitialiseComponent()
        {
            //this.components = new Container();
            this.titleLabel = new CaptionLabel();
            this.captionStrip = new CaptionToolStrip();
            this.captionBtns = new ToolStripButton[4];

            this.captionStrip.SuspendLayout();
            this.SuspendLayout();

            this.titleLabel.BackColor = Color.Transparent;
            this.titleLabel.Dock = DockStyle.Fill;
            this.titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.titleLabel.Margin = Padding.Empty;

            this.captionStrip.BackColor = Color.Transparent;
            this.captionStrip.Dock = DockStyle.Right;
            this.captionStrip.Font = new Font("Marlett", 12F);
            this.captionStrip.GripStyle = ToolStripGripStyle.Hidden;
            this.captionStrip.Padding = Padding.Empty;
            this.captionStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.captionStrip.Renderer = new Renderer();
            this.captionStrip.Margin = Padding.Empty;
            this.captionStrip.GripMargin = Padding.Empty;

            for (var i = 0; i < this.captionBtns.Length; i++)
            {
                var btn = this.captionBtns[i] = new ToolStripButton();
                btn.Alignment = ToolStripItemAlignment.Right;
                btn.DisplayStyle = ToolStripItemDisplayStyle.Text;
                btn.Margin = Padding.Empty;
                btn.Padding = new Padding(4, 0, 4, 0);
                btn.Overflow = ToolStripItemOverflow.Never;
                btn.TextAlign = ContentAlignment.MiddleCenter;
                btn.Click += captionBtns_Click;
            }

            this.captionBtns[BTN_MINIMISE].Text = '0'.ToString();
            this.captionBtns[BTN_MINIMISE].ToolTipText = "Minimise";
            this.captionBtns[BTN_MINIMISE].Tag = FormWindowState.Minimized;

            this.captionBtns[BTN_MAXIMISE].Text = '1'.ToString();
            this.captionBtns[BTN_MAXIMISE].ToolTipText = "Maximise";
            this.captionBtns[BTN_MAXIMISE].Tag = FormWindowState.Maximized;

            this.captionBtns[BTN_RESTORE].Text = '2'.ToString();
            this.captionBtns[BTN_RESTORE].ToolTipText = "Restore Down";
            this.captionBtns[BTN_RESTORE].Visible = false;
            this.captionBtns[BTN_RESTORE].Tag = FormWindowState.Normal;

            this.captionBtns[BTN_CLOSE].BackColor = Color.Tomato;
            this.captionBtns[BTN_CLOSE].Text = 'r'.ToString();
            this.captionBtns[BTN_CLOSE].ToolTipText = "Close";

            this.captionStrip.Items.AddRange(this.captionBtns);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.captionStrip);

            this.captionStrip.ResumeLayout(true);
            this.ResumeLayout(true);
        }

        #region Parent Notifications

        bool OnParentWmActivate(ref Message m, BaseWndProc baseProc)
        {
            UpdateForeColour(m.WParam.ToInt32() != NativeMethods.WA_INACTIVE);
            this.Refresh();
            return false;
        }

        bool OnParentWmSize(ref Message m, BaseWndProc baseProc)
        {
            var oldRect = this.ParentForm.ClientRectangle;
            baseProc(ref m);

            IntPtr hwnd = m.HWnd;
            var newRect = new NativeMethods.RECT();
            UnsafeNativeMethods.GetClientRect(hwnd, ref newRect);
            ActionOnFrameRect(oldRect, rect => UnsafeNativeMethods.InvalidateRect(hwnd, ref rect, 1));
            ActionOnFrameRect(newRect.ToRectangle(), rect => UnsafeNativeMethods.InvalidateRect(hwnd, ref rect, 1));
            return true;
        }

        bool OnParentWmPaint(ref Message m, BaseWndProc baseProc)
        {
            IntPtr hwnd = m.HWnd;
            var clientRect = new NativeMethods.RECT();
            UnsafeNativeMethods.GetClientRect(hwnd, ref clientRect);
            ActionOnFrameRect(clientRect.ToRectangle(), rect => UnsafeNativeMethods.ValidateRect(hwnd, ref rect));
            return false;
        }

        bool OnParentWmEraseBackground(ref Message m, BaseWndProc baseProc)
        {
            baseProc(ref m);

            using (var g = Graphics.FromHdc(m.WParam))
            {
                DrawParentFrame(g, m.HWnd);
            }
            return true;
        }

        bool OnParentWmWindowPosChanged(ref Message m, BaseWndProc baseProc)
        {
            if (this.ParentForm == null || this.ParentForm.IsHandleCreated == false)
            {
                return false;
            }

            // recover the correct size of the parent window
            // after the Form's WM_WINDOWPOSCHANGED handler messes it up.

            // 1a. minimise             FormWindowState.Normal, SW_SHOWMINIMIZED
            // 1b. maximise             FormWindowState.Maximized, SW_SHOWMAXIMIZED
            // 2. restore (ignore)      FormWindowState.Minimized, SW_SHOWMINIMIZED
            // 3. reentrant (<<here)    FormWindowState.Normal, SW_SHOWNORMAL

            var parentForm = this.ParentForm;
            var wp = new NativeMethods.WINDOWPLACEMENT();
            wp.length = Marshal.SizeOf(typeof(NativeMethods.WINDOWPLACEMENT));
            UnsafeNativeMethods.GetWindowPlacement(this.ParentForm.Handle, ref wp);

            var saveSize = (wp.showCmd == NativeMethods.SW_SHOWMINIMIZED)
                        || (wp.showCmd == NativeMethods.SW_SHOWMAXIMIZED);
            if (saveSize)
            {
                this.restoredWindowSize = wp.rcNormalPosition.ToRectangle().Size;
                this.restoredSizeSaved = true;
            }

            var stage2 = parentForm.WindowState == FormWindowState.Minimized && wp.showCmd == NativeMethods.SW_SHOWMINIMIZED;
            var stage3 = parentForm.WindowState == FormWindowState.Normal && wp.showCmd == NativeMethods.SW_SHOWNORMAL;

            baseProc(ref m);

            if (this.restoredSizeSaved && stage3)
            {
                this.restoredSizeSaved = false;
                //parentForm.Refresh();
                parentForm.Size = this.restoredWindowSize;
            }
            else if (stage2)
            {
            }
            return true;
        }

        bool OnParentWmNcCalcSize(ref Message m, BaseWndProc baseProc)
        {
            if (m.WParam != IntPtr.Zero)
            {
                m.Result = IntPtr.Zero;
                ExtendFrameIntoClientArea(m.HWnd);
                return true;
            }
            return false;
        }

        bool OnParentWmNcHitTest(ref Message m, BaseWndProc baseProc)
        {
            int hitTestCode = GetHitTest(m);
            if (hitTestCode != NativeMethods.HTNOWHERE)
            {
                m.Result = (IntPtr)hitTestCode;
                return true;
            }
            return false;
        }

        bool OnParentWmNcRButtonUp(ref Message m, BaseWndProc baseProc)
        {
            switch (m.WParam.ToInt32())
            {
                case NativeMethods.HTCAPTION:
                case NativeMethods.HTSYSMENU:
                    // HACK: display a system menu
                    UnsafeNativeMethods.PostMessage(m.HWnd, 0x313, IntPtr.Zero, m.LParam);
                    return true;
            }
            return false;
        }

        bool OnParentWmDwmCompositionChanged(ref Message m, BaseWndProc baseProc)
        {
            ExtendFrameIntoClientArea(m.HWnd);
            return false;
        }

        #endregion

        void DrawParentFrame(Graphics g, IntPtr hwnd)
        {
            var rect = new NativeMethods.RECT();
            UnsafeNativeMethods.GetClientRect(hwnd, ref rect);
            var frameRect = rect.ToRectangle();
            frameRect.Width--;
            frameRect.Height--;
            using (var pen = new Pen(this.BackColor))
            {
                g.DrawRectangle(pen, frameRect);
            }
        }

        void ExtendFrameIntoClientArea(IntPtr handle)
        {
            var margin = new NativeMethods.MARGINS(1);
            UnsafeNativeMethods.DwmExtendFrameIntoClientArea(handle, ref margin);
        }

        void ActionOnFrameRect(Rectangle rect, Action<NativeMethods.RECT> action)
        {
            var leftRect = new NativeMethods.RECT(rect.Left, rect.Top, rect.Left + 1, rect.Bottom);
            var topRect = new NativeMethods.RECT(rect.Left, rect.Top, rect.Right, rect.Top + 1);
            var rightRect = new NativeMethods.RECT(rect.Right - 1, rect.Top, rect.Right, rect.Bottom);
            var bottomRect = new NativeMethods.RECT(rect.Left, rect.Bottom - 1, rect.Right, rect.Bottom);
            action(leftRect);
            action(topRect);
            action(rightRect);
            action(bottomRect);
        }

        NativeMethods.RECT CalcParentWindowRect(IntPtr hwnd)
        {
            var adjustRect = new NativeMethods.RECT();
            var style = UnsafeNativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_STYLE);
            var exStyle = UnsafeNativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
            UnsafeNativeMethods.AdjustWindowRectEx(ref adjustRect, style & ~NativeMethods.WS_CAPTION, 0, exStyle);
            var windowRect = new NativeMethods.RECT();
            UnsafeNativeMethods.GetWindowRect(hwnd, ref windowRect);
            windowRect.left -= adjustRect.left;
            windowRect.top -= adjustRect.top;
            windowRect.right -= adjustRect.right;
            windowRect.bottom -= adjustRect.bottom;
            return windowRect;
        }

        int GetHitTest(Message m)
        {
            if (DesignMode == false)
            {
                var pt = new Point(m.LParam.ToInt32());
                var windowRect = CalcParentWindowRect(m.HWnd);

                //Debug.Print("{0},{1} ({2},{3})-({4},{5})", pt.X, pt.Y, rect.X, rect.Y, rect.Right, rect.Bottom);

                if (pt.X < windowRect.left)
                {
                    if (pt.Y < windowRect.top)
                    {
                        return NativeMethods.HTTOPLEFT;
                    }
                    else if (pt.Y > windowRect.bottom)
                    {
                        return NativeMethods.HTBOTTOMLEFT;
                    }
                    else
                    {
                        return NativeMethods.HTLEFT;
                    }
                }
                else if (pt.X > windowRect.right)
                {
                    if (pt.Y < windowRect.top)
                    {
                        return NativeMethods.HTTOPRIGHT;
                    }
                    else if (pt.Y > windowRect.bottom)
                    {
                        return NativeMethods.HTBOTTOMRIGHT;
                    }
                    else
                    {
                        return NativeMethods.HTRIGHT;
                    }
                }
                else if (pt.Y < windowRect.top)
                {
                    return NativeMethods.HTTOP;
                }
                else if (pt.Y > windowRect.bottom)
                {
                    return NativeMethods.HTBOTTOM;
                }

                var captionHeight = this.Height;
                if (pt.Y <= windowRect.top + captionHeight)
                {
                    var iconSize = SystemInformation.SmallIconSize;
                    var iconPadding = Math.Max(0, Math.Min((windowRect.right - windowRect.left - iconSize.Width) / 2, (captionHeight - iconSize.Height) / 2));
                    var iconRect = new Rectangle(windowRect.left, windowRect.top, iconSize.Width + iconPadding * 2, captionHeight);
                    if (iconRect.Contains(pt))
                    {
                        return NativeMethods.HTSYSMENU;
                    }
                    else
                    {
                        return NativeMethods.HTCAPTION;
                    }
                }
            }

            // return HTNOWHERE to forward base.WndProc()
            return NativeMethods.HTNOWHERE;
        }

        void UpdateForeColour()
        {
            UpdateForeColour(Form.ActiveForm != null && Form.ActiveForm.Equals(this.ParentForm));
        }

        void UpdateForeColour(bool active)
        {
            var brightness = this.BackColor.GetBrightness();
            var foreColourBase = ColourUtils.IsDark(this.BackColor) ? Color.White : Color.Black;
            if (active)
            {
                this.foreColour = foreColourBase;
            }
            else
            {
                this.foreColour = ColourUtils.Blend(this.BackColor, foreColourBase, .25f);
            }
            this.titleLabel.ForeColor = this.foreColour;
            this.captionStrip.ForeColor = this.foreColour;
            OnForeColorChanged(EventArgs.Empty);
        }

        void captionBtns_Click(object sender, EventArgs e)
        {
            var btn = (ToolStripButton)sender;
            if (btn.Tag is FormWindowState)
            {
                this.ParentForm.WindowState = (FormWindowState)btn.Tag;
                return;
            }
            else
            {
                this.ParentForm.Close();
            }
        }

        void ParentForm_TextChanged(object sender, EventArgs e)
        {
            titleLabel.Text = ((Form)sender).Text;
        }

        #region Overrides

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_NCHITTEST)
            {
                if (DesignMode == false)
                {
                    m.Result = (IntPtr)NativeMethods.HTTRANSPARENT;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            var parentForm = this.ParentForm;

            if (DesignMode == false)
            {
                this.parentListener = new ParentWindowListener(this, parentForm);
                parentForm.Padding += new Padding(1);
            }

            parentForm.TextChanged += ParentForm_TextChanged;
            ParentForm_TextChanged(parentForm, EventArgs.Empty);

            OnBackColorChanged(EventArgs.Empty);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);

            for (var i = 0; i < this.captionBtns.Length; i++)
            {
                if (i == BTN_CLOSE)
                {
                    this.captionBtns[i].BackColor = ColourUtils.Blend(this.BackColor, Color.Red, .125f);
                }
                else
                {
                    this.captionBtns[i].BackColor = ColourUtils.Blend(this.BackColor, Color.White, .75f);
                }
            }
            UpdateForeColour();

            if (DesignMode == false && this.ParentForm != null && this.ParentForm.Handle != null)
            {
                IntPtr hwnd = this.ParentForm.Handle;
                using (var g = Graphics.FromHwnd(hwnd))
                {
                    DrawParentFrame(g, hwnd);
                }
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.titleLabel.Font = this.Font;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (this.ParentForm != null)
            {
                this.captionStrip.SuspendLayout();
                var maximised = this.ParentForm.WindowState == FormWindowState.Maximized;
                this.captionBtns[BTN_MAXIMISE].Visible = maximised == false;
                this.captionBtns[BTN_RESTORE].Visible = maximised;
                this.captionStrip.ResumeLayout(true);
            }
        }

        #endregion

    }
}
