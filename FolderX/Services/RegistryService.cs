using Microsoft.Win32;
using System;

namespace FolderX.Services
{
    /// <summary>
    /// Shell Bags 缓存管理。
    ///
    /// 背景：Windows Explorer 会把每个文件夹的视图模式、列宽、排序等用户偏好缓存到注册表的
    ///   HKCU\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags
    ///   HKCU\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU
    ///   HKCU\Software\Microsoft\Windows\Shell\Bags
    ///   HKCU\Software\Microsoft\Windows\Shell\BagMRU
    ///
    /// 当 Bags 缓存存在时，Explorer 会优先使用缓存，导致修改 desktop.ini 后视图不刷新。
    /// 本服务提供清空 Bags 缓存的能力，强制 Explorer 重新读取 desktop.ini。
    ///
    /// 注：这是一个全局操作（影响所有文件夹的视图记忆），属于高级/可选功能。
    /// </summary>
    public class RegistryService
    {
        private static readonly string[] BagsRoots =
        {
            @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell",
            @"Software\Microsoft\Windows\Shell"
        };

        /// <summary>
        /// 检查是否存在 Shell Bags 缓存（任一根路径下的 Bags / BagMRU 子键）。
        /// </summary>
        public bool HasBagsCache()
        {
            foreach (var root in BagsRoots)
            {
                using var key = Registry.CurrentUser.OpenSubKey(root, writable: false);
                if (key == null) continue;
                if (key.OpenSubKey("Bags") != null) return true;
                if (key.OpenSubKey("BagMRU") != null) return true;
            }
            return false;
        }

        /// <summary>
        /// 估算缓存项数量（Bags 子键数 + BagMRU 子键数，所有根路径汇总）。
        /// </summary>
        public int GetBagsCount()
        {
            int total = 0;
            foreach (var root in BagsRoots)
            {
                using var key = Registry.CurrentUser.OpenSubKey(root, writable: false);
                if (key == null) continue;

                using (var bags = key.OpenSubKey("Bags"))
                    if (bags != null) total += bags.SubKeyCount;
                using (var bagMru = key.OpenSubKey("BagMRU"))
                    if (bagMru != null) total += bagMru.SubKeyCount;
            }
            return total;
        }

        /// <summary>
        /// 清空 Shell Bags 缓存（所有根路径下的 Bags / BagMRU 子树）。
        /// 调用方应先得到用户确认。重启 Explorer 后生效。
        /// </summary>
        public void ClearBagsCache()
        {
            foreach (var root in BagsRoots)
            {
                using var key = Registry.CurrentUser.OpenSubKey(root, writable: true);
                if (key == null) continue;

                TryDeleteSubKeyTree(key, "Bags");
                TryDeleteSubKeyTree(key, "BagMRU");
            }
        }

        private static void TryDeleteSubKeyTree(RegistryKey parent, string subKeyName)
        {
            try
            {
                if (parent.OpenSubKey(subKeyName) != null)
                    parent.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);
            }
            catch
            {
                // 忽略单个键删除失败，继续其他根路径
            }
        }
    }
}
