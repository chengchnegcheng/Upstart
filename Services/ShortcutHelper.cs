using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace StartupManager.Services
{
    /// <summary>
    /// 快捷方式帮助类
    /// </summary>
    public static class ShortcutHelper
    {
        /// <summary>
        /// 创建快捷方式
        /// </summary>
        /// <param name="shortcutPath">快捷方式路径</param>
        /// <param name="targetPath">目标程序路径</param>
        /// <param name="arguments">启动参数</param>
        /// <param name="description">描述</param>
        /// <returns>是否成功</returns>
        public static bool CreateShortcut(string shortcutPath, string targetPath, string arguments = "", string description = "")
        {
            try
            {
                // 使用PowerShell创建快捷方式
                var script = $@"
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
$Shortcut.TargetPath = '{targetPath}'
$Shortcut.Arguments = '{arguments}'
$Shortcut.WorkingDirectory = '{Path.GetDirectoryName(targetPath)}'
$Shortcut.Description = '{description}'
$Shortcut.Save()
";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{script}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(processInfo))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"创建快捷方式失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 解析快捷方式信息
        /// </summary>
        /// <param name="shortcutPath">快捷方式路径</param>
        /// <returns>快捷方式信息</returns>
        public static ShortcutInfo ParseShortcut(string shortcutPath)
        {
            try
            {
                // 使用PowerShell解析快捷方式
                var script = $@"
$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
Write-Output ""TargetPath:$($Shortcut.TargetPath)""
Write-Output ""Arguments:$($Shortcut.Arguments)""
Write-Output ""Description:$($Shortcut.Description)""
";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{script}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(processInfo))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        var info = new ShortcutInfo();
                        var lines = output.Split('\n');
                        
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("TargetPath:"))
                                info.TargetPath = line.Substring(11).Trim();
                            else if (line.StartsWith("Arguments:"))
                                info.Arguments = line.Substring(10).Trim();
                            else if (line.StartsWith("Description:"))
                                info.Description = line.Substring(12).Trim();
                        }
                        
                        return info;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析快捷方式失败: {ex.Message}");
            }

            return null;
        }
    }

    /// <summary>
    /// 快捷方式信息
    /// </summary>
    public class ShortcutInfo
    {
        public string TargetPath { get; set; } = "";
        public string Arguments { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
