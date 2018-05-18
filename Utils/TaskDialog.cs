using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using Debug = System.Diagnostics.Debug;

namespace Cool
{
    /// <summary>
    /// Specify the return value of a task dialog.
    /// </summary>
    public enum TaskDialogResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
        Close = 8,

        //
        UserButton1 = 101,
        UserButton2 = 102,
        UserButton3 = 103,
        UserButton4 = 104,
        UserButton5 = 105,
        UserButton6 = 106,
        UserButton7 = 107,
        UserButton8 = 108,
        UserButton9 = 109,
    }

    /// <summary>
    /// Specify which buttons to display.
    /// </summary>
    [Flags]
    public enum TaskDialogCommonButtons
    {
        None = 0,
        OK = 1,
        Yes = 2,
        No = 4,
        Cancel = 8,
        Retry = 16,
        Close = 32
    }

    /// <summary>
    /// Specify which icon to display.
    /// </summary>
    public enum TaskDialogIcon
    {
        None = 0,
        Warning = -1,
        Error = -2,
        Information = -3,
        Shield = -4
    }

    /// <summary>
    /// Specify the appearance of a task dialog.
    /// </summary>
    public enum TaskDialogAppearance
    {
        Button,
        CommandLinks,
        CommandLinksNoIcon,
    }

    /// <summary>
    /// Specify the appearance of the footer area of a task dialog.
    /// </summary>
    public enum TaskDialogFooterAppearance
    {
        Default,
        Expandable,
        Expanded
    }

    /// <summary>
    /// Specify a custom button to display.
    /// </summary>
    public class TaskDialogButton
    {
        public TaskDialogButton(TaskDialogResult result, string text, bool def = false)
        {
            this.DialogResult = result;
            this.Text = text;
            this.Default = def;
        }

        public TaskDialogResult DialogResult { get; set; }
        public string Text { get; set; }
        public bool Default { get; set; }
    }

    /// <summary>
    /// Display a task dialog.
    /// </summary>
    public class TaskDialog
    {
        readonly List<TaskDialogButton> dialogButtons = new List<TaskDialogButton>();
        TaskDialogCommonButtons commonButtons = TaskDialogCommonButtons.None;
        string windowTitle;
        TaskDialogIcon mainIcon = TaskDialogIcon.None;
        string mainInstruction;
        string contentText;
        string verificationText;
        string expandedInformation;
        string expandedControlText;
        string collapsedControlText;
        TaskDialogIcon footerIcon = TaskDialogIcon.None;
        string footerText;
        int width = 0;

        bool allowCancellation = false;
        TaskDialogAppearance appearance = TaskDialogAppearance.Button;
        TaskDialogFooterAppearance footerAppearance = TaskDialogFooterAppearance.Default;
        bool relativeToWindow = true;
        bool rightToLeft = false;
        bool canBeMinimised = false;
        bool sizeToContent = false;

        bool verificationChecked = false;

        #region P/Invoke

        static class NativeMethods
        {
            /// <summary>
            /// Functionally the same as MAKEINTRESOURCE macro.
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            internal static IntPtr MAKEINTRESOURCE(int id)
            {
                return new IntPtr(id & 0xffff);
            }

            // TASKDIALOGCONFIG.dwFlags
            //internal const uint TDF_ENABLE_HYPERLINKS = 0x0001;
            //internal const uint TDF_USE_HICON_MAIN = 0x0002;
            //internal const uint TDF_USE_HICON_FOOTER = 0x0004;
            internal const uint TDF_ALLOW_DIALOG_CANCELLATION = 0x0008;
            internal const uint TDF_USE_COMMAND_LINKS = 0x0010;
            internal const uint TDF_USE_COMMAND_LINKS_NO_ICON = 0x0020;
            internal const uint TDF_EXPAND_FOOTER_AREA = 0x0040;
            internal const uint TDF_EXPANDED_BY_DEFAULT = 0x0080;
            //internal const uint TDF_VERIFICATION_FLAG_CHECKED = 0x0100;
            //internal const uint TDF_SHOW_PROGRESS_BAR = 0x0200;
            //internal const uint TDF_SHOW_MARQUEE_PROGRESS_BAR = 0x0400;
            //internal const uint TDF_CALLBACK_TIMER = 0x0800;
            internal const uint TDF_POSITION_RELATIVE_TO_WINDOW = 0x1000;
            internal const uint TDF_RTL_LAYOUT = 0x2000;
            //internal const uint TDF_NO_DEFAULT_RADIO_BUTTON = 0x4000;
            internal const uint TDF_CAN_BE_MINIMIZED = 0x8000;
            //internal const uint TDF_NO_SET_FOREGROUND = 0x00010000;   // win8+
            internal const uint TDF_SIZE_TO_CONTENT = 0x01000000;

            internal static readonly IntPtr TD_WARNING_ICON = MAKEINTRESOURCE(-1);
            internal static readonly IntPtr TD_ERROR_ICON = MAKEINTRESOURCE(-2);
            internal static readonly IntPtr TD_INFORMATION_ICON = MAKEINTRESOURCE(-3);
            internal static readonly IntPtr TD_SHIELD_ICON = MAKEINTRESOURCE(-4);
        }

        static class UnsafeNativeMethods
        {
            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack=4)]
            internal class TASKDIALOG_BUTTON
            {
                public int nButtonID;
                public string pszButtonText;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
            internal class TASKDIALOGCONFIG
            {
                public int cbSize = Sizeof();
                public IntPtr hwndParent;
                public IntPtr _hInstance;           // never used
                public uint dwFlags;
                public uint dwCommonButtons;
                public string pszWindowTitle;
                public IntPtr pszMainIcon;
                public string pszMainInstruction;
                public string pszContent;
                public uint cButtons;
                public IntPtr pButtons;
                public int nDefaultButton;
                public uint _cRadioButtons;         // never used
                public IntPtr _pRadioButtons;       // never used
                public int _nDefaultRadioButton;    // never used
                public string pszVerificationText;
                public string pszExpandedInformation;
                public string pszExpandedControlText;
                public string pszCollapsedControlText;
                public IntPtr pszFooterIcon;
                public string pszFooter;
                public IntPtr _pfCallback;          // never used
                public IntPtr _lpCallbackData;      // never used
                public uint cxWidth;

                public static int Sizeof()
                {
                    return Marshal.SizeOf(typeof(TASKDIALOGCONFIG));
                }
            }

            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern IntPtr GetActiveWindow();

            [DllImport("comctl32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
            internal static extern int TaskDialog(IntPtr hwndOwner, IntPtr hInstance, string pszWindowTitle, string pszMainInstruction, string pszContent, uint dwCommonButtons, IntPtr pszIcon, out int pnButton);

            [DllImport("comctl32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
            internal static extern int TaskDialogIndirect(TASKDIALOGCONFIG pTaskConfig, out int pnButton, IntPtr pnRadioButton, [MarshalAs(UnmanagedType.Bool)] out bool pfVerificationFlagChecked);
        }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public TaskDialog()
        {
        }

        #region Properties

        [
            Category(CategoryNames.Behavior),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
            //Editor(typeof(CriterionCollectionEditor), typeof(System.Drawing.Design.UITypeEditor)),
            Description(""),
            MergableProperty(false)
        ]
        public List<TaskDialogButton> Buttons
        {
            get { return this.dialogButtons; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue(TaskDialogCommonButtons.None)
        ]
        public TaskDialogCommonButtons CommonButtons
        {
            get { return this.commonButtons; }
            set { this.commonButtons = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue("")
        ]
        public string WindowTitle
        {
            get { return this.windowTitle; }
            set { this.windowTitle = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue(TaskDialogIcon.None)
        ]
        public TaskDialogIcon MainIcon
        {
            get { return this.mainIcon; }
            set { this.mainIcon = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue("")
        ]
        public string MainInstruction
        {
            get { return this.mainInstruction; }
            set { this.mainInstruction = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue("")
        ]
        public string ContentText
        {
            get { return this.contentText; }
            set { this.contentText = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue("")
        ]
        public string VerificationText
        {
            get { return this.verificationText; }
            set { this.verificationText = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue("")
        ]
        public string ExpandedInformation
        {
            get { return this.expandedInformation; }
            set { this.expandedInformation = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue("")
        ]
        public string ExpandedControlText
        {
            get { return this.expandedControlText; }
            set { this.expandedControlText = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue("")
        ]
        public string CollapsedControlText
        {
            get { return this.collapsedControlText; }
            set { this.collapsedControlText = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue(TaskDialogIcon.None)
        ]
        public TaskDialogIcon FooterIcon
        {
            get { return this.footerIcon; }
            set { this.footerIcon = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue("")
        ]
        public string FooterText
        {
            get { return this.footerText; }
            set { this.footerText = value; }
        }

        [
            Category(CategoryNames.Behavior),
            Description(""),
            DefaultValue(false)
        ]
        public bool AllowCancellation
        {
            get { return this.allowCancellation; }
            set { this.allowCancellation = value; }
        }

        [
            Category(CategoryNames.Behavior),
            Description(""),
            DefaultValue(TaskDialogAppearance.Button)
        ]
        public TaskDialogAppearance Appearance
        {
            get { return this.appearance; }
            set { this.appearance = value; }
        }

        [
            Category(CategoryNames.Behavior),
            Description(""),
            DefaultValue(TaskDialogFooterAppearance.Default)
        ]
        public TaskDialogFooterAppearance FooterAppearance
        {
            get { return this.footerAppearance; }
            set { this.footerAppearance = value; }
        }

        [
            Category(CategoryNames.Layout),
            Description(""),
            DefaultValue(true)
        ]
        public bool PositionRelativeToWindow
        {
            get { return this.relativeToWindow; }
            set { this.relativeToWindow = value; }
        }

        [
            Category(CategoryNames.Appearance),
            Description(""),
            DefaultValue(false)
        ]
        public bool RightToLeft
        {
            get { return this.rightToLeft; }
            set { this.rightToLeft = value; }
        }

        [
            Category(CategoryNames.Behavior),
            Description(""),
            DefaultValue(false)
        ]
        public bool CanBeMinimized
        {
            get { return this.canBeMinimised; }
            set { this.canBeMinimised = value; }
        }

        [
            Category(CategoryNames.Layout),
            Description(""),
            DefaultValue(false)
        ]
        public bool SizeToContent
        {
            get { return this.sizeToContent; }
            set { this.sizeToContent = value; }
        }

        [
            Category(CategoryNames.Layout),
            Description(""),
            DefaultValue(0)
        ]
        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        [
            Category(CategoryNames.Misc),
            Description(""),
            DefaultValue(false)
        ]
        public bool VerificationChecked
        {
            get { return this.verificationChecked; }
            set { this.verificationChecked = value; }
        }

        #endregion Properties

        /// <summary>
        /// Display a task dialog.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public TaskDialogResult ShowDialog(IWin32Window owner)
        {
            int defaultButton;
            var win32Btns = ToButtons(this.Buttons, out defaultButton);

            int hr, buttonId;
            bool verificationChecked;

            try
            {
                var config = new UnsafeNativeMethods.TASKDIALOGCONFIG();
                config.hwndParent = GetOwnerWindow(owner);
                config.dwFlags = PropertiesToFlags();
                config.dwCommonButtons = SanitiseFlags(this.commonButtons);
                config.pszWindowTitle = this.windowTitle;
                config.pszMainIcon = IconToID(this.mainIcon);
                config.pszMainInstruction = this.mainInstruction;
                config.pszContent = this.contentText;
                config.cButtons = (uint)this.Buttons.Count;
                config.pButtons = win32Btns;
                config.nDefaultButton = defaultButton;
                config.pszVerificationText = this.verificationText;
                config.pszExpandedInformation = this.expandedInformation;
                config.pszExpandedControlText = this.expandedControlText;
                config.pszCollapsedControlText = this.collapsedControlText;
                config.pszFooterIcon = IconToID(this.footerIcon);
                config.pszFooter = this.footerText;
                config.cxWidth = this.width >= 0 ? (uint)this.width : 0;

                hr = UnsafeNativeMethods.TaskDialogIndirect(config, out buttonId, IntPtr.Zero, out verificationChecked);
            }
            finally
            {
                Marshal.FreeCoTaskMem(win32Btns);
            }

            this.verificationChecked = false;
            if (hr >= 0)
            {
                this.verificationChecked = verificationChecked;
            }
            else
            {
                throw new COMException("TaskDialogIndirect() failed.", hr);
            }

            return ResultFromKnownButtonID(buttonId);
        }

        /// <summary>
        /// Display a task dialog.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="title"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(IWin32Window owner, string title, string instruction)
        {
            return Show(owner, title, instruction, null, TaskDialogCommonButtons.OK, TaskDialogIcon.None);
        }

        /// <summary>
        /// Display a task dialog.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="title"></param>
        /// <param name="instruction"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(IWin32Window owner, string title, string instruction, TaskDialogCommonButtons buttons)
        {
            return Show(owner, title, instruction, null, buttons, TaskDialogIcon.None);
        }

        /// <summary>
        /// Display a task dialog.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="title"></param>
        /// <param name="instruction"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(IWin32Window owner, string title, string instruction, TaskDialogCommonButtons buttons, TaskDialogIcon icon)
        {
            return Show(owner, title, instruction, null, buttons, icon);
        }

        /// <summary>
        /// Display a task dialog.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="title"></param>
        /// <param name="instruction"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(IWin32Window owner, string title, string instruction, string content)
        {
            return Show(owner, title, instruction, content, TaskDialogCommonButtons.OK, TaskDialogIcon.None);
        }

        /// <summary>
        /// Display a task dialog.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="title"></param>
        /// <param name="instruction"></param>
        /// <param name="content"></param>
        /// <param name="buttons"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(IWin32Window owner, string title, string instruction, string content, TaskDialogCommonButtons buttons)
        {
            return Show(owner, title, instruction, content, buttons, TaskDialogIcon.None);
        }

        /// <summary>
        /// Display a task dialog.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="title"></param>
        /// <param name="instruction"></param>
        /// <param name="content"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(IWin32Window owner, string title, string instruction, string content, TaskDialogCommonButtons buttons, TaskDialogIcon icon)
        {
            int buttonId = 0;
            int hr = UnsafeNativeMethods.TaskDialog(GetOwnerWindow(owner), IntPtr.Zero, title, instruction, content, SanitiseFlags(buttons), IconToID(icon), out buttonId);
            if (hr != 0)
            {
                throw new COMException("TaskDialog() failed.", hr);
            }
            return ResultFromKnownButtonID(buttonId);
        }

        uint PropertiesToFlags()
        {
            uint flags = 0;

            if (this.allowCancellation)
            {
                flags |= NativeMethods.TDF_ALLOW_DIALOG_CANCELLATION;
            }

            switch (this.appearance)
            {
                case TaskDialogAppearance.Button:
                    break;
                case TaskDialogAppearance.CommandLinks:
                    flags |= NativeMethods.TDF_USE_COMMAND_LINKS;
                    break;
                case TaskDialogAppearance.CommandLinksNoIcon:
                    flags |= NativeMethods.TDF_USE_COMMAND_LINKS | NativeMethods.TDF_USE_COMMAND_LINKS_NO_ICON;
                    break;
            }

            switch (this.footerAppearance)
            {
                case TaskDialogFooterAppearance.Default:
                    break;
                case TaskDialogFooterAppearance.Expandable:
                    flags |= NativeMethods.TDF_EXPAND_FOOTER_AREA;
                    break;
                case TaskDialogFooterAppearance.Expanded:
                    flags |= NativeMethods.TDF_EXPAND_FOOTER_AREA | NativeMethods.TDF_EXPANDED_BY_DEFAULT;
                    break;
            }

            if (this.relativeToWindow)
            {
                flags |= NativeMethods.TDF_POSITION_RELATIVE_TO_WINDOW;
            }
            if (this.rightToLeft)
            {
                flags |= NativeMethods.TDF_RTL_LAYOUT;
            }
            if (this.canBeMinimised)
            {
                flags |= NativeMethods.TDF_CAN_BE_MINIMIZED;
            }
            if (this.sizeToContent)
            {
                flags |= NativeMethods.TDF_SIZE_TO_CONTENT;
            }
            return flags;
        }

        static IntPtr GetOwnerWindow(IWin32Window owner)
        {
            if (owner != null && owner.Handle != null)
            {
                return owner.Handle;
            }
            return UnsafeNativeMethods.GetActiveWindow();
        }

        static uint SanitiseFlags(TaskDialogCommonButtons commonButtons)
        {
            const uint COMMONBUTTONS_VALIDBITS = 0x2F;
            return (uint)commonButtons & COMMONBUTTONS_VALIDBITS;
        }

        static IntPtr IconToID(TaskDialogIcon icon)
        {
            switch (icon)
            {
                case TaskDialogIcon.None:
                    return IntPtr.Zero;
                case TaskDialogIcon.Warning:
                    return NativeMethods.TD_WARNING_ICON;
                case TaskDialogIcon.Error:
                    return NativeMethods.TD_ERROR_ICON;
                case TaskDialogIcon.Information:
                    return NativeMethods.TD_INFORMATION_ICON;
                case TaskDialogIcon.Shield:
                    return NativeMethods.TD_SHIELD_ICON;
            }
            return IntPtr.Zero;
        }

        static IntPtr ToButtons(List<TaskDialogButton> buttons, out int defaultButton)
        {
            defaultButton = 0;
            var count = buttons.Count;
            var sizeOfStruct = Marshal.SizeOf(typeof(UnsafeNativeMethods.TASKDIALOG_BUTTON));
            var bytes = checked(count * sizeOfStruct);
            var win32btns = Marshal.AllocCoTaskMem(bytes);
            var ptr = win32btns;
            var i = 0;
            foreach (var btn in buttons)
            {
                if (btn.Default)
                {
                    defaultButton = (int)btn.DialogResult;
                }
                var tdb = new UnsafeNativeMethods.TASKDIALOG_BUTTON();
                tdb.nButtonID = (int)btn.DialogResult;
                tdb.pszButtonText = btn.Text;
                Marshal.StructureToPtr(tdb, ptr, false);
                ptr += sizeOfStruct;
                i++;
            }
            return win32btns;
        }

        /// <summary>
        /// Convert Win32 button ID to the corresponding TaskDialogResult enum.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static TaskDialogResult ResultFromKnownButtonID(int id)
        {
            switch (id)
            {
                case 0:
                    return TaskDialogResult.None;
                case 1: // IDOK
                    return TaskDialogResult.OK;
                case 2: // IDCANCEL
                    return TaskDialogResult.Cancel;
                case 3: // IDABORT
                    return TaskDialogResult.Abort;
                case 4: // IDRETRY
                    return TaskDialogResult.Retry;
                case 5: // IDIGNORE
                    return TaskDialogResult.Ignore;
                case 6: // IDYES
                    return TaskDialogResult.Yes;
                case 7: // IDNO
                    return TaskDialogResult.No;
                case 8: // IDCLOSE
                    return TaskDialogResult.Close;
                default:
                    Debug.Print("TaskDialog() returned a strange button id: {0}", id);
                    return TaskDialogResult.None;
            }
        }
    }
}
