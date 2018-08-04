using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cool
{
    static class ShellUtils
    {
        public enum StockIcon : int
        {
            DocNoAssoc = 0,
            DocAssoc = 1,
            Application = 2,
            Folder = 3,
            FolderOpen = 4,
            Drive525 = 5,
            Drive35 = 6,
            DriveRemove = 7,
            DriveFixed = 8,
            DriveNet = 9,
            DriveNetDisabled = 10,
            DriveCD = 11,
            DriveRAM = 12,
            World = 13,
            Server = 15,
            Printer = 16,
            MyNetwork = 17,
            Find = 22,
            Help = 23,
            Share = 28,
            Link = 29,
            SlowFile = 30,
            Recycler = 31,
            RecyclerFull = 32,
            MediaCDAudio = 40,
            Lock = 47,
            AutoList = 49,
            PrinterNet = 50,
            ServerShare = 51,
            PrinterFax = 52,
            PrinterFaxNet = 53,
            PrinterFile = 54,
            Stack = 55,
            MediaSVCD = 56,
            StuffedFolder = 57,
            DriveUnknown = 58,
            DriveDVD = 59,
            MediaDVD = 60,
            MediaDVDRAM = 61,
            MediaDVDRW = 62,
            MediaDVDR = 63,
            MediaDVDROM = 64,
            MediaCDAudioPlus = 65,
            MediaCDRW = 66,
            MediaCDR = 67,
            MediaCDBurn = 68,
            MediaBlankCD = 69,
            MediaCDROM = 70,
            AudioFiles = 71,
            ImageFiles = 72,
            VideoFiles = 73,
            MixedFiles = 74,
            FolderBack = 75,
            FolderFront = 76,
            Shield = 77,
            Warning = 78,
            Info = 79,
            Error = 80,
            Key = 81,
            Software = 82,
            Rename = 83,
            Delete = 84,
            MediaAudioDVD = 85,
            MediaMovieDVD = 86,
            MediaEnhancedCD = 87,
            MediaEnhancedDVD = 88,
            MediaHDDVD = 89,
            MediaBluRay = 90,
            MediaVCD = 91,
            MediaDVDPlusR = 92,
            MediaDVDPlusRW = 93,
            DesktopPC = 94,
            MobilePC = 95,
            Users = 96,
            MediaSmartMedia = 97,
            MediaCompactFlash = 98,
            DeviceCellPhone = 99,
            DeviceCamera = 100,
            DeviceVideoCamera = 101,
            DeviceAudioPlayer = 102,
            NetworkConnect = 103,
            Internet = 104,
            ZipFile = 105,
            Settings = 106,
            DriveHDDVD = 132,
            DriveBD = 133,
            MediaHDDVDROM = 134,
            MediaHDDVDR = 135,
            MediaHDDVDRAM = 136,
            MediaBDROM = 137,
            MediaBDR = 138,
            MediaBDRE = 139,
            ClusteredDrive = 140,
        }

        public enum IconSize
        {
            Large,
            Small,
            ShellSize,
        }

        [Flags]
        public enum StockIconOptions
        {
            LinkOverlay = 1,
            Selected = 2,
        }

        public static Icon ExtractIcon(string filePath, IconSize size = IconSize.Large)
        {
            IntPtr hicoLarge, hicoSmall;
            if (UnsafeNativeMethods.ExtractIconEx(filePath, 0, out hicoLarge, out hicoSmall, 1) == 2)
            {
                var icon = CreateIconFromHandle(size == IconSize.Small ? hicoSmall : hicoLarge);
                UnsafeNativeMethods.DestroyIcon(hicoLarge);
                UnsafeNativeMethods.DestroyIcon(hicoSmall);
                return icon;
            }
            else
            {
                // load a stock icon because the app doesn't have an icon.
                return GetStockIcon(StockIcon.Application, size);
            }
        }

        public static Icon GetStockIcon(StockIcon stockIcon, IconSize size = IconSize.Large, StockIconOptions options = 0)
        {
            uint flags = ToStockIconFlags(size, options);
            UnsafeNativeMethods.SHSTOCKICONINFO sii = new UnsafeNativeMethods.SHSTOCKICONINFO();
            sii.cbSize = UnsafeNativeMethods.SHSTOCKICONINFO.Sizeof();
            int hr = UnsafeNativeMethods.SHGetStockIconInfo((int)stockIcon, flags, ref sii);
            if (hr != 0)
            {
                throw new COMException("SHGetStockIconInfo failed.", hr);
            }
            if (sii.hIcon == IntPtr.Zero)
            {
                throw new NotSupportedException("The stock icon is not supported by the running OS.");
            }

            var icon = CreateIconFromHandle(sii.hIcon);
            UnsafeNativeMethods.DestroyIcon(sii.hIcon);
            return icon;
        }

        private static Icon CreateIconFromHandle(IntPtr hico)
        {
            Icon newIcon;
            using (var tmpIcon = Icon.FromHandle(hico))
            {
                newIcon = new Icon(tmpIcon, tmpIcon.Size);
            }
            return newIcon;
        }

        private static uint ToStockIconFlags(IconSize size, StockIconOptions options)
        {
            uint flags = NativeMethods.SHGSI_ICON;
            switch (size)
            {
                case IconSize.Large:
                    flags |= NativeMethods.SHGSI_LARGEICON;
                    break;
                case IconSize.Small:
                    flags |= NativeMethods.SHGSI_SMALLICON;
                    break;
                case IconSize.ShellSize:
                    flags |= NativeMethods.SHGSI_SHELLICONSIZE;
                    break;
            }
            if (options.HasFlag(StockIconOptions.LinkOverlay))
            {
                flags |= NativeMethods.SHGSI_LINKOVERLAY;
            }
            if (options.HasFlag(StockIconOptions.Selected))
            {
                flags |= NativeMethods.SHGSI_SELECTED;
            }
            return flags;
        }

        static class NativeMethods
        {
            //internal const uint SHGSI_ICONLOCATION = 0;
            internal const uint SHGSI_ICON = 0x100;
            //internal const uint SHGSI_SYSICONINDEX = 0x4000;
            internal const uint SHGSI_LINKOVERLAY = 0x8000;
            internal const uint SHGSI_SELECTED = 0x10000;
            internal const uint SHGSI_LARGEICON = 0;
            internal const uint SHGSI_SMALLICON = 0x1;
            internal const uint SHGSI_SHELLICONSIZE = 0x4;
        }

        static class UnsafeNativeMethods
        {
            [DllImport("user32.dll", ExactSpelling = true)]
            internal static extern int DestroyIcon(IntPtr hico);

            [DllImport("shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode)]
            internal static extern int ExtractIconEx(string lpszFile, int nIconIndex, out IntPtr phiconLarge, out IntPtr phiconSmall, uint nIcons);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct SHSTOCKICONINFO
            {
                public int cbSize;
                public IntPtr hIcon;
                public int iSysImageIndex;
                public int iIcon;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szPath;

                public static int Sizeof()
                {
                    return Marshal.SizeOf(typeof(SHSTOCKICONINFO));
                }
            }

            [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
            internal static extern int SHGetStockIconInfo(int siid, uint uFlags, ref SHSTOCKICONINFO psii);
        }
    }
}
