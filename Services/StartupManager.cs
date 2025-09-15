using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StartupManager.Models;
using System.Diagnostics;
using System.Text;

namespace StartupManager.Services
{
    /// <summary>
    /// 启动项管理服务
    /// </summary>
    public class StartupManagerService
    {
        /// <summary>
        /// 当前用户启动文件夹路径
        /// </summary>
        public static string UserStartupPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            @"Microsoft\Windows\Start Menu\Programs\Startup");

        /// <summary>
        /// 所有用户启动文件夹路径
        /// </summary>
        public static string AllUsersStartupPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"Microsoft\Windows\Start Menu\Programs\Startup");

        /// <summary>
        /// 获取所有启动项
        /// </summary>
        /// <returns>启动项列表</returns>
        public List<StartupItem> GetStartupItems()
        {
            var items = new List<StartupItem>();

            // 获取用户级启动项
            items.AddRange(GetStartupItemsFromFolder(UserStartupPath, false));

            // 获取系统级启动项（只读）
            items.AddRange(GetStartupItemsFromFolder(AllUsersStartupPath, true));

            return items.OrderBy(x => x.Name).ToList();
        }

        /// <summary>
        /// 从指定文件夹获取启动项
        /// </summary>
        /// <param name="folderPath">文件夹路径</param>
        /// <param name="isSystemLevel">是否为系统级</param>
        /// <returns>启动项列表</returns>
        private List<StartupItem> GetStartupItemsFromFolder(string folderPath, bool isSystemLevel)
        {
            var items = new List<StartupItem>();

            if (!Directory.Exists(folderPath))
                return items;

            try
            {
                var files = Directory.GetFiles(folderPath, "*.lnk");
                foreach (var file in files)
                {
                    var item = ParseShortcut(file, isSystemLevel);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取启动项时发生错误: {ex.Message}");
            }

            return items;
        }

        /// <summary>
        /// 解析快捷方式文件
        /// </summary>
        /// <param name="shortcutPath">快捷方式路径</param>
        /// <param name="isSystemLevel">是否为系统级</param>
        /// <returns>启动项信息</returns>
        private StartupItem ParseShortcut(string shortcutPath, bool isSystemLevel)
        {
            try
            {
                var shortcutInfo = ShortcutHelper.ParseShortcut(shortcutPath);
                if (shortcutInfo != null)
                {
                    var item = new StartupItem
                    {
                        Name = Path.GetFileNameWithoutExtension(shortcutPath),
                        Path = shortcutInfo.TargetPath,
                        Arguments = shortcutInfo.Arguments,
                        ShortcutPath = shortcutPath,
                        IsEnabled = true,
                        DateAdded = System.IO.File.GetCreationTime(shortcutPath)
                    };

                    return item;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"解析快捷方式失败 {shortcutPath}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 添加启动项
        /// </summary>
        /// <param name="name">程序名称</param>
        /// <param name="targetPath">目标程序路径</param>
        /// <param name="arguments">启动参数</param>
        /// <returns>是否成功</returns>
        public bool AddStartupItem(string name, string targetPath, string arguments = "")
        {
            try
            {
                if (!System.IO.File.Exists(targetPath))
                {
                    throw new FileNotFoundException("目标程序文件不存在");
                }

                var shortcutPath = Path.Combine(UserStartupPath, $"{name}.lnk");

                // 确保启动文件夹存在
                Directory.CreateDirectory(UserStartupPath);

                // 创建快捷方式
                return ShortcutHelper.CreateShortcut(shortcutPath, targetPath, arguments, $"{name} - 开机启动");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"添加启动项失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 删除启动项
        /// </summary>
        /// <param name="item">要删除的启动项</param>
        /// <returns>是否成功</returns>
        public bool RemoveStartupItem(StartupItem item)
        {
            try
            {
                if (System.IO.File.Exists(item.ShortcutPath))
                {
                    System.IO.File.Delete(item.ShortcutPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"删除启动项失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查程序是否已在启动项中
        /// </summary>
        /// <param name="targetPath">程序路径</param>
        /// <returns>是否存在</returns>
        public bool IsInStartup(string targetPath)
        {
            var items = GetStartupItems();
            return items.Any(x => string.Equals(x.Path, targetPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}
