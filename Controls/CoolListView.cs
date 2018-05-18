using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Cool
{
    /// <summary>
    /// Provide a "cool" listview.
    /// </summary>
    public class CoolListView : ListView
    {
        #region P/Invoke

        static class NativeMethods
        {
            public const uint WM_UPDATEUISTATE = 0x0128;
            public const int UIS_SET_HIDEFOCUS_ACCEL = 0x30001;
        }

        static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", EntryPoint = "SendMessageW")]
            internal static extern IntPtr SendMessage(IntPtr hwnd, uint message, int wParam, int lParam);
        }

        #endregion

        public CoolListView()
        {
            base.DoubleBuffered = true;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            HideAnnoyingFocusRectangles();
            // the Explorer-style listview looks cooler than the standard listview.
            WindowUtils.SetExplorerStyleControl(this);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            HideAnnoyingFocusRectangles();
        }

        /// <summary>
        /// Hide annoying focus rectangles.
        /// </summary>
        void HideAnnoyingFocusRectangles()
        {
            UnsafeNativeMethods.SendMessage(this.Handle, NativeMethods.WM_UPDATEUISTATE, NativeMethods.UIS_SET_HIDEFOCUS_ACCEL, 0);
        }
    }
}
