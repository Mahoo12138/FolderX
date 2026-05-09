using System;

namespace FolderX.Models
{
    public class FolderInfo
    {
        public string FolderPath { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public int IconIndex { get; set; } = 0;
        public bool IsReadOnly { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSystem { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ModifiedTime { get; set; }

        // desktop.ini 显示设置
        public string ViewMode { get; set; } = "详细信息";
        public string SortBy { get; set; } = "名称";
        public string GroupBy { get; set; } = "无";
        public string SortDirection { get; set; } = "从左到右";
        public bool ShowHiddenFiles { get; set; }
        public bool ShowFileExtensions { get; set; } = true;
        public bool ShowSystemFiles { get; set; }
    }
}
