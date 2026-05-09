using FolderX.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace FolderX
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;

            _vm.BrowseFolderRequested      += OnBrowseFolderRequested;
            _vm.ChangeIconRequested        += OnChangeIconRequested;
            _vm.ExtractSystemIconRequested += OnExtractSystemIconRequested;
        }

        private void OnBrowseFolderRequested(object? sender, EventArgs e)
        {
            var dlg = new OpenFolderDialog
            {
                Title = "选择文件夹",
                Multiselect = false
            };
            if (!string.IsNullOrEmpty(_vm.FolderPath) && Directory.Exists(_vm.FolderPath))
                dlg.InitialDirectory = _vm.FolderPath;

            if (dlg.ShowDialog(this) == true)
            {
                _vm.LoadFolder(dlg.FolderName);
            }
        }

        private void OnChangeIconRequested(object? sender, EventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "选择图标文件",
                Filter = "图标与可执行文件 (*.ico;*.dll;*.exe)|*.ico;*.dll;*.exe|图标文件 (*.ico)|*.ico|所有文件 (*.*)|*.*"
            };
            if (dlg.ShowDialog(this) == true)
            {
                _vm.IconFilePath = dlg.FileName;
                _vm.UseCustomIcon = true;
                var icon = new Services.IconService().ExtractIconFromFile(dlg.FileName, 0);
                if (icon != null) _vm.IconPreviewImage = icon;
            }
        }

        private void OnExtractSystemIconRequested(object? sender, EventArgs e)
        {
            string sysDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var dlg = new OpenFileDialog
            {
                Title = "从系统图标库选择",
                Filter = "图标资源 (*.dll;*.exe;*.ico)|*.dll;*.exe;*.ico|所有文件 (*.*)|*.*",
                InitialDirectory = sysDir,
                FileName = "shell32.dll"
            };
            if (dlg.ShowDialog(this) == true)
            {
                _vm.IconFilePath = dlg.FileName;
                _vm.UseCustomIcon = true;
                var icon = new Services.IconService().ExtractIconFromFile(dlg.FileName, 0);
                if (icon != null) _vm.IconPreviewImage = icon;
            }
        }

        private void FolderPathBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Directory.Exists(_vm.FolderPath))
            {
                _vm.LoadFolder(_vm.FolderPath);
            }
        }

        // ──────────────────────────── 拖拽支持 ────────────────────────────

        private static readonly string[] IconExtensions = { ".ico", ".dll", ".exe" };

        /// <summary>
        /// 窗口级拖入：文件夹 → 加载；图标文件 → 设为自定义图标
        /// </summary>
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = TryGetDropTarget(e, out _, out _) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!TryGetDropTarget(e, out string path, out bool isFolder)) return;

            if (isFolder)
            {
                _vm.LoadFolder(path);
            }
            else
            {
                ApplyCustomIcon(path);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 图标预览区拖入：仅接受图标文件
        /// </summary>
        private void IconPreview_DragOver(object sender, DragEventArgs e)
        {
            bool ok = TryGetDropTarget(e, out _, out bool isFolder) && !isFolder;
            e.Effects = ok ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void IconPreview_Drop(object sender, DragEventArgs e)
        {
            if (TryGetDropTarget(e, out string path, out bool isFolder) && !isFolder)
            {
                ApplyCustomIcon(path);
                e.Handled = true;
            }
        }

        /// <summary>
        /// 解析拖入项：取首项，根据是文件夹还是图标文件分发；其它类型返回 false。
        /// </summary>
        private static bool TryGetDropTarget(DragEventArgs e, out string path, out bool isFolder)
        {
            path = string.Empty;
            isFolder = false;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return false;
            if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0) return false;

            string first = files[0];
            if (Directory.Exists(first))
            {
                path = first;
                isFolder = true;
                return true;
            }

            if (File.Exists(first))
            {
                string ext = Path.GetExtension(first).ToLowerInvariant();
                if (Array.IndexOf(IconExtensions, ext) >= 0)
                {
                    path = first;
                    isFolder = false;
                    return true;
                }
            }
            return false;
        }

        private void ApplyCustomIcon(string iconFile)
        {
            _vm.IconFilePath = iconFile;
            _vm.UseCustomIcon = true;
            var icon = new Services.IconService().ExtractIconFromFile(iconFile, 0);
            if (icon != null) _vm.IconPreviewImage = icon;
            _vm.StatusText = $"已将图标设为：{iconFile}";
        }
    }
}