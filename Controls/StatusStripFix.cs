using System;
using System.Windows.Forms;
using System.Drawing;

namespace Cool
{
    public class StatusStripFix : StatusStrip
    {
        static class NativeMethods
        {
            public const int WM_NCHITTEST = 0x0084;
            public const int HTBOTTOMLEFT = 16;
            public const int HTBOTTOMRIGHT = 17;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_NCHITTEST && this.SizingGrip && DesignMode == false)
            {
                var pt = new Point(m.LParam.ToInt32());
                var gripRect = RectangleToScreen(this.SizeGripBounds);
                if (gripRect.Contains(pt))
                {
                    var hitTest = this.RightToLeft == RightToLeft.Yes
                                ? NativeMethods.HTBOTTOMLEFT
                                : NativeMethods.HTBOTTOMRIGHT;
                    m.Result = new IntPtr(hitTest);
                    return;
                }
            }
            base.WndProc(ref m);
        }
    }
}
