using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FolderX.Services
{
    public class IconService
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(string lpszFile, int nIconIndex,
            IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        /// <summary>
        /// 获取文件夹的系统默认大图标（BitmapSource，用于 WPF Image）
        /// </summary>
        public BitmapSource? GetFolderIcon(string folderPath, bool large = true)
        {
            var shfi = new SHFILEINFO();
            uint flags = SHGFI_ICON | (large ? SHGFI_LARGEICON : SHGFI_SMALLICON);

            // 先尝试真实路径（包含自定义图标）
            IntPtr ret = SHGetFileInfo(folderPath, 0, ref shfi, (uint)Marshal.SizeOf(shfi), flags);
            if (ret == IntPtr.Zero || shfi.hIcon == IntPtr.Zero)
            {
                // 回退：使用通用文件夹图标
                ret = SHGetFileInfo(folderPath, FILE_ATTRIBUTE_DIRECTORY, ref shfi,
                    (uint)Marshal.SizeOf(shfi), flags | SHGFI_USEFILEATTRIBUTES);
            }

            if (shfi.hIcon == IntPtr.Zero) return null;

            try
            {
                return HIconToBitmapSource(shfi.hIcon);
            }
            finally
            {
                DestroyIcon(shfi.hIcon);
            }
        }

        /// <summary>
        /// 从文件中提取指定索引的图标（.ico / .dll / .exe）
        /// </summary>
        public BitmapSource? ExtractIconFromFile(string filePath, int iconIndex)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return null;

            IntPtr[] large = new IntPtr[1];
            IntPtr[] small = new IntPtr[1];

            try
            {
                uint count = ExtractIconEx(filePath, iconIndex, large, small, 1);
                if (count == 0 || large[0] == IntPtr.Zero) return null;
                return HIconToBitmapSource(large[0]);
            }
            finally
            {
                if (large[0] != IntPtr.Zero) DestroyIcon(large[0]);
                if (small[0] != IntPtr.Zero) DestroyIcon(small[0]);
            }
        }

        /// <summary>
        /// 将 HICON 转换为 WPF BitmapSource
        /// </summary>
        public BitmapSource HIconToBitmapSource(IntPtr hIcon)
        {
            var source = Imaging.CreateBitmapSourceFromHIcon(
                hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
    }
}
