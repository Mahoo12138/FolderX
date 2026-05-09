using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FolderX.Services
{
    public class DesktopIniService
    {
        private const string IniFileName = "desktop.ini";

        // ────────────────────────────────── 读取 ──────────────────────────────────

        public Dictionary<string, Dictionary<string, string>> Read(string folderPath)
        {
            var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            string iniPath = Path.Combine(folderPath, IniFileName);
            if (!File.Exists(iniPath)) return result;

            string? currentSection = null;
            // StreamReader 自动检测 BOM，兼容 UTF-16 LE / UTF-8 with BOM / 系统 ANSI
            using var sr = new StreamReader(iniPath, detectEncodingFromByteOrderMarks: true);
            while (sr.ReadLine() is { } rawLine)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(';') || line.StartsWith('#'))
                    continue;

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    currentSection = line[1..^1];
                    if (!result.ContainsKey(currentSection))
                        result[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else if (currentSection != null)
                {
                    int eq = line.IndexOf('=');
                    if (eq > 0)
                    {
                        string key = line[..eq].Trim();
                        string val = line[(eq + 1)..].Trim();
                        result[currentSection][key] = val;
                    }
                }
            }
            return result;
        }

        public string GetValue(string folderPath, string section, string key, string defaultValue = "")
        {
            var data = Read(folderPath);
            if (data.TryGetValue(section, out var sec) && sec.TryGetValue(key, out var val))
                return val;
            return defaultValue;
        }

        // ────────────────────────────────── 写入 ──────────────────────────────────

        /// <summary>
        /// 将完整的节/键值字典写入 desktop.ini，同时设置文件夹的 System 属性及 desktop.ini 的 Hidden+System 属性。
        /// </summary>
        public void Write(string folderPath, Dictionary<string, Dictionary<string, string>> data)
        {
            string iniPath = Path.Combine(folderPath, IniFileName);

            var sb = new StringBuilder();
            foreach (var (section, pairs) in data)
            {
                sb.AppendLine($"[{section}]");
                foreach (var (key, val) in pairs)
                    sb.AppendLine($"{key}={val}");
                sb.AppendLine();
            }

            // 先去除隐藏/只读属性，才能写入
            if (File.Exists(iniPath))
            {
                var attrs = File.GetAttributes(iniPath);
                File.SetAttributes(iniPath, attrs & ~(FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System));
            }

            File.WriteAllText(iniPath, sb.ToString(), new UnicodeEncoding(bigEndian: false, byteOrderMark: true));

            // 标记 desktop.ini 为 Hidden + System
            File.SetAttributes(iniPath, FileAttributes.Hidden | FileAttributes.System);

            // 标记文件夹为 System（使 desktop.ini 对 Explorer 生效）
            var folderAttrs = File.GetAttributes(folderPath);
            File.SetAttributes(folderPath, folderAttrs | FileAttributes.System);
        }

        // ────────────────────────────────── 辅助操作 ──────────────────────────────

        public bool Exists(string folderPath)
            => File.Exists(Path.Combine(folderPath, IniFileName));

        public long GetFileSize(string folderPath)
        {
            string iniPath = Path.Combine(folderPath, IniFileName);
            return File.Exists(iniPath) ? new FileInfo(iniPath).Length : 0;
        }

        public DateTime GetLastWriteTime(string folderPath)
        {
            string iniPath = Path.Combine(folderPath, IniFileName);
            return File.Exists(iniPath) ? File.GetLastWriteTime(iniPath) : DateTime.MinValue;
        }

        public string GetIniPath(string folderPath)
            => Path.Combine(folderPath, IniFileName);

        /// <summary>
        /// 备份 desktop.ini 为 desktop.ini.bak
        /// </summary>
        public void Backup(string folderPath)
        {
            string iniPath = Path.Combine(folderPath, IniFileName);
            if (!File.Exists(iniPath)) return;
            string bakPath = iniPath + ".bak";

            if (File.Exists(bakPath))
                File.SetAttributes(bakPath, FileAttributes.Normal);

            // 清除隐藏/系统属性才能复制
            File.SetAttributes(iniPath, FileAttributes.Normal);
            File.Copy(iniPath, bakPath, overwrite: true);
            File.SetAttributes(iniPath, FileAttributes.Hidden | FileAttributes.System);
        }

        /// <summary>
        /// 从备份文件还原 desktop.ini
        /// </summary>
        public void Restore(string folderPath)
        {
            string bakPath = Path.Combine(folderPath, IniFileName + ".bak");
            if (!File.Exists(bakPath)) return;
            string iniPath = Path.Combine(folderPath, IniFileName);

            if (File.Exists(iniPath))
                File.SetAttributes(iniPath, FileAttributes.Normal);

            File.Copy(bakPath, iniPath, overwrite: true);
            File.SetAttributes(iniPath, FileAttributes.Hidden | FileAttributes.System);
        }

        /// <summary>
        /// 删除 desktop.ini，并移除文件夹的 System 属性
        /// </summary>
        public void Delete(string folderPath)
        {
            string iniPath = Path.Combine(folderPath, IniFileName);
            if (File.Exists(iniPath))
            {
                File.SetAttributes(iniPath, FileAttributes.Normal);
                File.Delete(iniPath);
            }

            // 移除文件夹 System 属性
            var folderAttrs = File.GetAttributes(folderPath);
            File.SetAttributes(folderPath, folderAttrs & ~FileAttributes.System);
        }

        // ────────────────────────────────── ShellClassInfo 专用 ───────────────────

        public (string iconPath, int iconIndex, string infoTip) ReadShellClassInfo(string folderPath)
        {
            var data = Read(folderPath);
            if (!data.TryGetValue(".ShellClassInfo", out var sci))
                return (string.Empty, 0, string.Empty);

            string iconResource = sci.GetValueOrDefault("IconResource", string.Empty);
            string infoTip = sci.GetValueOrDefault("InfoTip", string.Empty);

            // IconResource 格式：path,index
            string iconPath = string.Empty;
            int iconIndex = 0;
            if (!string.IsNullOrEmpty(iconResource))
            {
                int comma = iconResource.LastIndexOf(',');
                if (comma >= 0)
                {
                    iconPath = iconResource[..comma].Trim();
                    int.TryParse(iconResource[(comma + 1)..].Trim(), out iconIndex);
                }
                else
                {
                    iconPath = iconResource.Trim();
                }
            }
            return (iconPath, iconIndex, infoTip);
        }

        public void WriteShellClassInfo(string folderPath, string iconPath, int iconIndex, string infoTip)
        {
            var data = Read(folderPath);
            if (!data.TryGetValue(".ShellClassInfo", out var sci))
            {
                sci = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                data[".ShellClassInfo"] = sci;
            }

            if (!string.IsNullOrEmpty(iconPath))
                sci["IconResource"] = $"{iconPath},{iconIndex}";
            else
                sci.Remove("IconResource");

            if (!string.IsNullOrEmpty(infoTip))
                sci["InfoTip"] = infoTip;
            else
                sci.Remove("InfoTip");

            // 清理空节
            if (sci.Count == 0) data.Remove(".ShellClassInfo");

            Write(folderPath, data);
        }
    }
}
