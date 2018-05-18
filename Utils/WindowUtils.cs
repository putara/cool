using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Cool
{
    static class WindowUtils
    {
        public static void ExtendFrameIntoClientArea(IWin32Window window)
        {
            var margins = new NativeMethods.MARGINS(-1);
            UnsafeNativeMethods.DwmExtendFrameIntoClientArea(window.Handle, ref margins);
        }

        public static void ExtendFrameIntoClientArea(IWin32Window window, int left, int top, int right, int bottom)
        {
            var margins = new NativeMethods.MARGINS(left, top, right, bottom);
            UnsafeNativeMethods.DwmExtendFrameIntoClientArea(window.Handle, ref margins);
        }

        public static void SetExplorerStyleControl(IWin32Window window)
        {
            UnsafeNativeMethods.SetWindowTheme(window.Handle, "Explorer", IntPtr.Zero);
        }

        public static void SetTextBoxCueBanner(TextBoxBase textBox, string cueBannerText)
        {
            UnsafeNativeMethods.SendMessageString(textBox.Handle, NativeMethods.EM_SETCUEBANNER, new IntPtr(1), cueBannerText);
        }

        [Conditional("DEBUG")]
        public static void DebugPrintFocusedControlInfo()
        {
            var hwnd = UnsafeNativeMethods.GetFocus();
            if (hwnd != IntPtr.Zero)
            {
                uint msg = UnsafeNativeMethods.RegisterWindowMessage("WM_GETCONTROLTYPE");
                const int cch = 0x1000;
                var p = Marshal.AllocCoTaskMem(cch * 2);
                UnsafeNativeMethods.SendMessage(hwnd, msg, new IntPtr(cch), p);
                string s = Marshal.PtrToStringUni(p);
                Marshal.FreeCoTaskMem(p);
                if (s.Length > 0)
                {
                    var i = s.IndexOf(',');
                    s = i >= 0 ? s.Substring(0, i) : "???";
                }
                else
                {
                    s = "(Native)";
                }
                Debug.Print("0x{0:X} {1}", hwnd.ToInt32(), s);
            }
        }

        static class NativeMethods
        {
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

            private const uint ECM_FIRST = 0x1500;
            internal const uint EM_SETCUEBANNER = ECM_FIRST + 1;
        }

        static class UnsafeNativeMethods
        {
            [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
            internal static extern int SetWindowTheme(IntPtr hwnd, string className, IntPtr dontCare);

            [DllImport("dwmapi.dll", ExactSpelling = true)]
            internal static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref NativeMethods.MARGINS margins);

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern IntPtr GetFocus();

            [DllImport("user32.dll", EntryPoint = "SendMessageW")]
            internal static extern IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
            internal static extern IntPtr SendMessageString(IntPtr hwnd, uint message, IntPtr wParam, string lParam);

            [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW", CharSet = CharSet.Unicode)]
            internal static extern uint RegisterWindowMessage(string message);
        }
    }
}
