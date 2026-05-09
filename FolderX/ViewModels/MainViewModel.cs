using FolderX.Models;
using FolderX.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FolderX.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ─────────────────────────── 服务 ───────────────────────────
        private readonly DesktopIniService _iniService = new();
        private readonly IconService _iconService = new();
        private readonly RegistryService _registryService = new();

        // ─────────────────────────── 属性存储 ───────────────────────────
        private string _folderPath = string.Empty;
        private string _folderName = string.Empty;
        private string _comment = string.Empty;
        private BitmapSource? _folderIconImage;
        private BitmapSource? _iconPreviewImage;
        private string _iconFilePath = string.Empty;

        private bool _isReadOnly;
        private bool _isHidden;
        private bool _isSystem;
        private DateTime _createdTime;
        private DateTime _modifiedTime;

        // 图标设置
        private bool _useDefaultIcon = true;
        private bool _useCustomIcon;

        // 显示设置
        private string _viewMode = "详细信息";
        private string _sortBy = "名称";
        private string _groupBy = "无";
        private string _sortDirection = "从左到右";
        private bool _showHiddenFiles;
        private bool _showFileExtensions = true;
        private bool _showSystemFiles;

        // desktop.ini 状态
        private string _iniStatus = "不存在";
        private string _iniPath = string.Empty;
        private string _iniSize = string.Empty;
        private string _iniModifiedTime = string.Empty;

        // Shell Bags 状态
        private string _bagsStatus = "未检测";
        private string _bagsCount = "—";

        // 状态栏
        private string _statusText = "就绪";

        // 选中的列
        private ColumnSetting? _selectedColumn;

        // ─────────────────────────── 可绑定属性 ───────────────────────────

        public string FolderPath
        {
            get => _folderPath;
            set { _folderPath = value; OnPropertyChanged(); }
        }

        public string FolderName
        {
            get => _folderName;
            set { _folderName = value; OnPropertyChanged(); }
        }

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CommentLength));
            }
        }

        public int CommentLength => _comment.Length;

        public BitmapSource? FolderIconImage
        {
            get => _folderIconImage;
            set { _folderIconImage = value; OnPropertyChanged(); }
        }

        public BitmapSource? IconPreviewImage
        {
            get => _iconPreviewImage;
            set { _iconPreviewImage = value; OnPropertyChanged(); }
        }

        public string IconFilePath
        {
            get => _iconFilePath;
            set { _iconFilePath = value; OnPropertyChanged(); }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(); }
        }

        public bool IsHidden
        {
            get => _isHidden;
            set { _isHidden = value; OnPropertyChanged(); }
        }

        public bool IsSystem
        {
            get => _isSystem;
            set { _isSystem = value; OnPropertyChanged(); }
        }

        public DateTime CreatedTime
        {
            get => _createdTime;
            set { _createdTime = value; OnPropertyChanged(); }
        }

        public DateTime ModifiedTime
        {
            get => _modifiedTime;
            set { _modifiedTime = value; OnPropertyChanged(); }
        }

        // 图标选项
        public bool UseDefaultIcon
        {
            get => _useDefaultIcon;
            set
            {
                _useDefaultIcon = value;
                OnPropertyChanged();
                if (value) UseCustomIcon = false;
            }
        }

        public bool UseCustomIcon
        {
            get => _useCustomIcon;
            set
            {
                _useCustomIcon = value;
                OnPropertyChanged();
                if (value) UseDefaultIcon = false;
            }
        }

        // 显示设置
        public string ViewMode
        {
            get => _viewMode;
            set { _viewMode = value; OnPropertyChanged(); }
        }

        public string SortBy
        {
            get => _sortBy;
            set { _sortBy = value; OnPropertyChanged(); }
        }

        public string GroupBy
        {
            get => _groupBy;
            set { _groupBy = value; OnPropertyChanged(); }
        }

        public string SortDirection
        {
            get => _sortDirection;
            set { _sortDirection = value; OnPropertyChanged(); }
        }

        public bool ShowHiddenFiles
        {
            get => _showHiddenFiles;
            set { _showHiddenFiles = value; OnPropertyChanged(); }
        }

        public bool ShowFileExtensions
        {
            get => _showFileExtensions;
            set { _showFileExtensions = value; OnPropertyChanged(); }
        }

        public bool ShowSystemFiles
        {
            get => _showSystemFiles;
            set { _showSystemFiles = value; OnPropertyChanged(); }
        }

        // desktop.ini 状态
        public string IniStatus
        {
            get => _iniStatus;
            set { _iniStatus = value; OnPropertyChanged(); }
        }

        public string IniPath
        {
            get => _iniPath;
            set { _iniPath = value; OnPropertyChanged(); }
        }

        public string IniSize
        {
            get => _iniSize;
            set { _iniSize = value; OnPropertyChanged(); }
        }

        public string IniModifiedTime
        {
            get => _iniModifiedTime;
            set { _iniModifiedTime = value; OnPropertyChanged(); }
        }

        // Shell Bags 状态
        public string BagsStatus
        {
            get => _bagsStatus;
            set { _bagsStatus = value; OnPropertyChanged(); }
        }

        public string BagsCount
        {
            get => _bagsCount;
            set { _bagsCount = value; OnPropertyChanged(); }
        }

        // 状态栏
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public ColumnSetting? SelectedColumn
        {
            get => _selectedColumn;
            set { _selectedColumn = value; OnPropertyChanged(); }
        }

        // 下拉列表数据源
        public List<string> ViewModes { get; } = new() { "详细信息", "图标", "小图标", "列表", "平铺", "内容" };
        public List<string> SortByOptions { get; } = new() { "名称", "修改日期", "类型", "大小", "创建日期", "作者", "标记" };
        public List<string> GroupByOptions { get; } = new() { "无", "名称", "修改日期", "类型", "大小", "创建日期" };
        public List<string> SortDirections { get; } = new() { "从左到右", "从右到左" };

        // 列设置
        public ObservableCollection<ColumnSetting> Columns { get; } = new();

        // ─────────────────────────── 命令 ───────────────────────────
        public ICommand BrowseFolderCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SaveDesktopIniCommand { get; }
        public ICommand ApplyAllCommand { get; }
        public ICommand ExitCommand { get; }

        public ICommand ChangeIconCommand { get; }
        public ICommand ExtractSystemIconCommand { get; }
        public ICommand RestoreDefaultIconCommand { get; }

        public ICommand BackupIniCommand { get; }
        public ICommand RestoreIniCommand { get; }
        public ICommand DeleteIniCommand { get; }

        public ICommand ClearBagsCacheCommand { get; }
        public ICommand RefreshBagsStatusCommand { get; }

        public ICommand MoveColumnUpCommand { get; }
        public ICommand MoveColumnDownCommand { get; }
        public ICommand ShowColumnCommand { get; }
        public ICommand HideColumnCommand { get; }
        public ICommand ResetColumnsCommand { get; }

        // ─────────────────────────── 构造函数 ───────────────────────────
        public MainViewModel()
        {
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
            OpenFolderCommand = new RelayCommand(OpenFolder, () => Directory.Exists(_folderPath));
            RefreshCommand = new RelayCommand(Refresh, () => Directory.Exists(_folderPath));
            SaveDesktopIniCommand = new RelayCommand(SaveDesktopIni, () => Directory.Exists(_folderPath));
            ApplyAllCommand = new RelayCommand(ApplyAll, () => Directory.Exists(_folderPath));
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            ChangeIconCommand = new RelayCommand(ChangeIcon, () => Directory.Exists(_folderPath));
            ExtractSystemIconCommand = new RelayCommand(ExtractSystemIcon, () => Directory.Exists(_folderPath));
            RestoreDefaultIconCommand = new RelayCommand(RestoreDefaultIcon, () => Directory.Exists(_folderPath));

            BackupIniCommand = new RelayCommand(BackupIni, () => _iniService.Exists(_folderPath));
            RestoreIniCommand = new RelayCommand(RestoreIni, () => File.Exists(Path.Combine(_folderPath, "desktop.ini.bak")));
            DeleteIniCommand = new RelayCommand(DeleteIni, () => _iniService.Exists(_folderPath));

            ClearBagsCacheCommand    = new RelayCommand(ClearBagsCache);
            RefreshBagsStatusCommand = new RelayCommand(RefreshBagsStatus);

            MoveColumnUpCommand = new RelayCommand(MoveColumnUp, () => SelectedColumn != null && Columns.IndexOf(SelectedColumn) > 0);
            MoveColumnDownCommand = new RelayCommand(MoveColumnDown, () => SelectedColumn != null && Columns.IndexOf(SelectedColumn) < Columns.Count - 1);
            ShowColumnCommand = new RelayCommand(ShowColumn, () => SelectedColumn != null && !SelectedColumn.IsVisible);
            HideColumnCommand = new RelayCommand(HideColumn, () => SelectedColumn != null && SelectedColumn.IsVisible);
            ResetColumnsCommand = new RelayCommand(ResetColumns);

            InitDefaultColumns();
            RefreshBagsStatus();
        }

        // ─────────────────────────── 初始化列 ───────────────────────────
        private void InitDefaultColumns()
        {
            Columns.Clear();
            var defaults = new[]
            {
                new ColumnSetting("名称",     "System.ItemNameDisplay",  true,  0),
                new ColumnSetting("修改日期", "System.DateModified",     true,  1),
                new ColumnSetting("类型",     "System.ItemTypeText",     true,  2),
                new ColumnSetting("大小",     "System.Size",             true,  3),
                new ColumnSetting("创建日期", "System.DateCreated",      true,  4),
                new ColumnSetting("作者",     "System.Author",           true,  5),
                new ColumnSetting("标记",     "System.Keywords",         true,  6),
                new ColumnSetting("备注",     "System.Comment",          true,  7),
                new ColumnSetting("文件版本", "System.FileVersion",      true,  8),
                new ColumnSetting("产品名称", "System.Software.ProductName", true, 9),
            };
            foreach (var c in defaults) Columns.Add(c);
        }

        // ─────────────────────────── 文件夹加载 ───────────────────────────
        public void LoadFolder(string path)
        {
            if (!Directory.Exists(path)) return;
            FolderPath = path;

            var di = new DirectoryInfo(path);
            FolderName = di.Name;
            var attrs = di.Attributes;
            IsReadOnly = (attrs & FileAttributes.ReadOnly) != 0;
            IsHidden = (attrs & FileAttributes.Hidden) != 0;
            IsSystem = (attrs & FileAttributes.System) != 0;
            CreatedTime = di.CreationTime;
            ModifiedTime = di.LastWriteTime;

            // desktop.ini
            var (iconPath, iconIndex, infoTip) = _iniService.ReadShellClassInfo(path);
            Comment = infoTip;

            if (!string.IsNullOrEmpty(iconPath))
            {
                UseCustomIcon = true;
                IconFilePath = iconPath;
                IconPreviewImage = _iconService.ExtractIconFromFile(iconPath, iconIndex);
            }
            else
            {
                UseDefaultIcon = true;
                IconFilePath = string.Empty;
                IconPreviewImage = null;
            }

            // 文件夹图标（显示当前图标）
            FolderIconImage = _iconService.GetFolderIcon(path);

            RefreshIniStatus();
            RefreshBagsStatus();

            StatusText = $"已加载：{path}";
        }

        private void RefreshIniStatus()
        {
            if (_iniService.Exists(_folderPath))
            {
                IniStatus = "已存在";
                IniPath = _iniService.GetIniPath(_folderPath);
                long size = _iniService.GetFileSize(_folderPath);
                IniSize = $"{size / 1024.0:F2} KB";
                IniModifiedTime = _iniService.GetLastWriteTime(_folderPath).ToString("yyyy/MM/dd HH:mm:ss");
            }
            else
            {
                IniStatus = "不存在";
                IniPath = string.Empty;
                IniSize = string.Empty;
                IniModifiedTime = string.Empty;
            }
        }

        private void RefreshBagsStatus()
        {
            try
            {
                bool has = _registryService.HasBagsCache();
                BagsStatus = has ? "存在缓存" : "无缓存";
                BagsCount  = has ? _registryService.GetBagsCount().ToString() : "0";
            }
            catch
            {
                BagsStatus = "读取失败";
                BagsCount = "—";
            }
        }

        // ─────────────────────────── 命令实现 ───────────────────────────

        private void BrowseFolder()
        {
            // 通过事件请求 View 打开对话框
            BrowseFolderRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OpenFolder()
        {
            if (Directory.Exists(_folderPath))
                System.Diagnostics.Process.Start("explorer.exe", _folderPath);
        }

        private void Refresh()
        {
            if (!string.IsNullOrEmpty(_folderPath))
                LoadFolder(_folderPath);
        }

        private void SaveDesktopIni()
        {
            if (!Directory.Exists(_folderPath)) return;
            try
            {
                var data = _iniService.Read(_folderPath);

                // ShellClassInfo
                if (!data.ContainsKey(".ShellClassInfo"))
                    data[".ShellClassInfo"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                var sci = data[".ShellClassInfo"];
                if (_useCustomIcon && !string.IsNullOrEmpty(_iconFilePath))
                    sci["IconResource"] = $"{_iconFilePath},0";
                else
                    sci.Remove("IconResource");

                if (!string.IsNullOrEmpty(_comment))
                    sci["InfoTip"] = _comment;
                else
                    sci.Remove("InfoTip");

                if (sci.Count == 0) data.Remove(".ShellClassInfo");

                _iniService.Write(_folderPath, data);
                RefreshIniStatus();
                FolderIconImage = _iconService.GetFolderIcon(_folderPath);
                StatusText = "desktop.ini 已保存。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyAll()
        {
            if (!Directory.Exists(_folderPath)) return;
            try
            {
                // 1. 应用文件夹基本属性
                var di = new DirectoryInfo(_folderPath);
                var attrs = FileAttributes.Directory;
                if (_isReadOnly) attrs |= FileAttributes.ReadOnly;
                if (_isHidden) attrs |= FileAttributes.Hidden;
                if (_isSystem) attrs |= FileAttributes.System;
                di.Attributes = attrs;

                // 2. 保存 desktop.ini
                SaveDesktopIni();

                StatusText = "所有设置已应用。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用设置失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeIcon()
        {
            ChangeIconRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExtractSystemIcon()
        {
            // 由 View 打开系统图标库（shell32.dll 等）选择对话框
            ExtractSystemIconRequested?.Invoke(this, EventArgs.Empty);
        }

        private void RestoreDefaultIcon()
        {
            UseDefaultIcon = true;
            IconFilePath = string.Empty;
            IconPreviewImage = null;
            FolderIconImage = _iconService.GetFolderIcon(_folderPath);
            StatusText = "已标记恢复默认图标（保存后生效）。";
        }

        private void BackupIni()
        {
            if (!Directory.Exists(_folderPath)) return;
            try
            {
                _iniService.Backup(_folderPath);
                StatusText = "desktop.ini 已备份为 desktop.ini.bak。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"备份失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreIni()
        {
            if (!Directory.Exists(_folderPath)) return;
            try
            {
                _iniService.Restore(_folderPath);
                LoadFolder(_folderPath);
                StatusText = "desktop.ini 已从备份还原。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"还原失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteIni()
        {
            var result = MessageBox.Show("确定删除 desktop.ini 吗？此操作不可撤销。",
                "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                _iniService.Delete(_folderPath);
                RefreshIniStatus();
                StatusText = "desktop.ini 已删除。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearBagsCache()
        {
            var result = MessageBox.Show(
                "确定要清空 Shell Bags 缓存吗？\n\n" +
                "该操作会删除 Explorer 记忆的所有文件夹视图/列宽/排序偏好，常用于修改 desktop.ini 后强制刷新。\n" +
                "需重启 Explorer 后生效。",
                "确认清空缓存", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                _registryService.ClearBagsCache();
                RefreshBagsStatus();
                StatusText = "Shell Bags 缓存已清空。建议重启 Explorer。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清空缓存失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MoveColumnUp()
        {
            if (SelectedColumn == null) return;
            int idx = Columns.IndexOf(SelectedColumn);
            if (idx <= 0) return;
            Columns.Move(idx, idx - 1);
        }

        private void MoveColumnDown()
        {
            if (SelectedColumn == null) return;
            int idx = Columns.IndexOf(SelectedColumn);
            if (idx < 0 || idx >= Columns.Count - 1) return;
            Columns.Move(idx, idx + 1);
        }

        private void ShowColumn()
        {
            if (SelectedColumn == null) return;
            SelectedColumn.IsVisible = true;
            // 刷新列表显示
            int idx = Columns.IndexOf(SelectedColumn);
            Columns[idx] = SelectedColumn;
        }

        private void HideColumn()
        {
            if (SelectedColumn == null) return;
            SelectedColumn.IsVisible = false;
            int idx = Columns.IndexOf(SelectedColumn);
            Columns[idx] = SelectedColumn;
        }

        private void ResetColumns()
        {
            InitDefaultColumns();
        }

        // ─────────────────────────── 事件（用于 View 打开对话框）───────────────────────────
        public event EventHandler? BrowseFolderRequested;
        public event EventHandler? ChangeIconRequested;
        public event EventHandler? ExtractSystemIconRequested;

        // ─────────────────────────── INotifyPropertyChanged ───────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
