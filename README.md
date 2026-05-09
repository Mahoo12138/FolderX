# FolderX

FolderX 是一个用于编辑文件夹属性的小工具（WPF，.NET 8），支持修改 `desktop.ini`、自定义文件夹图标、视图/列设置，并提供拖拽使用体验。

---

## 主要功能

- 修改文件夹备注（InfoTip）、属性（只读/隐藏/系统）
- 自定义文件夹图标（支持 `.ico/.dll/.exe`，支持拖拽）
- 编辑并保存 `desktop.ini`（包括 `.ShellClassInfo`）
- 管理详细信息视图的列与顺序
- 清空 Explorer 的 Shell Bags 缓存（高级），解决 desktop.ini 修改后不刷新问题
- 支持通过拖拽快速加载文件夹或设定图标

---

## 要求

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022（推荐）或使用 `dotnet` CLI
- 可选：Python + Pillow（用于生成 `.ico`）

---

## 构建与运行

Visual Studio：打开 `FolderX.sln`，选择 `Debug` 后运行（F5）。

命令行（项目根）：

```bash
dotnet build FolderX.sln -c Debug
# 运行已构建的 exe：
# .\FolderX\bin\Debug\net8.0-windows\FolderX.exe
```

---

## 快速使用

1. 点击“浏览”或将文件夹拖入窗口以加载目标文件夹。
2. 在左侧编辑备注、属性；中间设置自定义图标（可浏览或拖入 `.ico/.dll/.exe`）。
3. 点击“保存 desktop.ini”保存更改。
4. 如果 Explorer 未立即反映修改，请在右侧使用“清空 Bags 缓存”并重启 Explorer。

---

## 生成应用图标（可选）

如果你有源 PNG，可以用 Python + Pillow 生成多尺寸 `.ico`：

```bash
python - << 'PY'
from PIL import Image
img = Image.open('app.png').convert('RGBA')
sizes = [(16,16),(32,32),(48,48),(64,64),(128,128),(256,256)]
img.save('app.ico', format='ICO', sizes=sizes)
PY
```

本仓库已经包含 `app.ico` 并在项目中配置为应用图标。

---

## 开发说明

- 代码结构：
  - `FolderX/Services`：`DesktopIniService`、`IconService`、`RegistryService`（Shell Bags 管理）
  - `FolderX/ViewModels`：`MainViewModel`、`RelayCommand`
  - `FolderX/Models`：数据模型
  - `FolderX/Converters`：UI 转换器
  - `MainWindow.xaml`：主 UI

---

## 贡献

欢迎提 PR 或 issue。请基于 `main` 分支创建 feature 分支并提交 PR。

