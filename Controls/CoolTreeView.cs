using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Cool
{
    /// <summary>
    /// Provide a "cool" treeview.
    /// </summary>
    public class CoolTreeView : TreeView
    {
        private bool fadeInOutExpandos = true;

        #region P/Invoke

        static class NativeMethods
        {
            public const int TVS_EX_DOUBLEBUFFER = 0x0004;
            public const int TVS_EX_FADEINOUTEXPANDOS = 0x0040;
            private const uint TV_FIRST = 0x1100;
            public const uint TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;
        }

        static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", EntryPoint = "SendMessageW")]
            internal static extern IntPtr SendMessage(IntPtr hwnd, uint message, int wParam, int lParam);
        }

        #endregion

        public CoolTreeView()
        {
            base.DoubleBuffered = true;
        }

        [
            Category(CategoryNames.Behavior),
            DefaultValue(true)
        ]
        public bool FadeInOutExpandos
        {
            get { return this.fadeInOutExpandos; }
            set
            {
                this.fadeInOutExpandos = value;
                this.SetExtendedStyles();
            }
        }

        private void SetExtendedStyles()
        {
            if (this.Handle != IntPtr.Zero)
            {
                int styles = NativeMethods.TVS_EX_DOUBLEBUFFER;
                if (this.fadeInOutExpandos)
                {
                    styles |= NativeMethods.TVS_EX_FADEINOUTEXPANDOS;
                }
                UnsafeNativeMethods.SendMessage(this.Handle, NativeMethods.TVM_SETEXTENDEDSTYLE, 0, styles);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            WindowUtils.HideAnnoyingFocusRectangles(this);
            // the Explorer-style listview looks cooler than the standard listview.
            WindowUtils.SetExplorerStyleControl(this);
            this.SetExtendedStyles();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            WindowUtils.HideAnnoyingFocusRectangles(this);
        }
    }
}
