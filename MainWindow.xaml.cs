using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StartupManager.Models;
using StartupManager.Services;
using StartupManager.Views;

namespace StartupManager
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly StartupManagerService _startupService;
        private ObservableCollection<StartupItem> _startupItems;
        private string _sortColumn = "";
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;

        public ObservableCollection<StartupItem> StartupItems
        {
            get => _startupItems;
            set
            {
                _startupItems = value;
                OnPropertyChanged(nameof(StartupItems));
                UpdateItemCount();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _startupService = new StartupManagerService();
            StartupItems = new ObservableCollection<StartupItem>();
            
            DataContext = this;
            LvStartupItems.ItemsSource = StartupItems;
            
            // 设置启动文件夹路径显示
            RunStartupPath.Text = StartupManagerService.UserStartupPath;
            
            LoadStartupItems();
        }

        /// <summary>
        /// 加载启动项列表
        /// </summary>
        private void LoadStartupItems()
        {
            try
            {
                TbStatus.Text = "正在加载启动项...";
                
                var items = _startupService.GetStartupItems();
                StartupItems.Clear();
                
                foreach (var item in items)
                {
                    StartupItems.Add(item);
                }
                
                TbStatus.Text = "加载完成";
                UpdateItemCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载启动项时发生错误：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TbStatus.Text = "加载失败";
            }
        }

        /// <summary>
        /// 更新项目计数
        /// </summary>
        private void UpdateItemCount()
        {
            TbItemCount.Text = $"共 {StartupItems.Count} 个启动项";
        }

        /// <summary>
        /// 添加程序按钮点击事件
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProgramDialog { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                var name = dialog.ProgramName;
                var path = dialog.ProgramPath;
                var arguments = dialog.Arguments;

                // 检查是否已存在
                if (_startupService.IsInStartup(path))
                {
                    MessageBox.Show("该程序已在启动项中！", "提示", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 添加启动项
                if (_startupService.AddStartupItem(name, path, arguments))
                {
                    TbStatus.Text = $"已添加启动项：{name}";
                    LoadStartupItems(); // 重新加载列表
                }
                else
                {
                    MessageBox.Show("添加启动项失败！", "错误", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    TbStatus.Text = "添加失败";
                }
            }
        }

        /// <summary>
        /// 刷新列表按钮点击事件
        /// </summary>
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStartupItems();
        }

        /// <summary>
        /// 打开启动文件夹按钮点击事件
        /// </summary>
        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", StartupManagerService.UserStartupPath);
                TbStatus.Text = "已打开启动文件夹";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开文件夹失败：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 删除按钮点击事件
        /// </summary>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is StartupItem item)
            {
                var result = MessageBox.Show($"确定要删除启动项 \"{item.Name}\" 吗？", "确认删除", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    if (_startupService.RemoveStartupItem(item))
                    {
                        StartupItems.Remove(item);
                        TbStatus.Text = $"已删除启动项：{item.Name}";
                        UpdateItemCount();
                    }
                    else
                    {
                        MessageBox.Show("删除启动项失败！", "错误", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        TbStatus.Text = "删除失败";
                    }
                }
            }
        }

        /// <summary>
        /// 列表头点击排序事件
        /// </summary>
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader headerClicked)
            {
                string sortBy = headerClicked.Column.Header.ToString();
                
                if (_sortColumn == sortBy)
                {
                    _sortDirection = _sortDirection == ListSortDirection.Ascending ? 
                        ListSortDirection.Descending : ListSortDirection.Ascending;
                }
                else
                {
                    _sortColumn = sortBy;
                    _sortDirection = ListSortDirection.Ascending;
                }

                SortList(sortBy, _sortDirection);
            }
        }

        /// <summary>
        /// 排序列表
        /// </summary>
        private void SortList(string sortBy, ListSortDirection direction)
        {
            var items = StartupItems.ToList();
            
            switch (sortBy)
            {
                case "程序名称":
                    items = direction == ListSortDirection.Ascending ? 
                        items.OrderBy(x => x.Name).ToList() : 
                        items.OrderByDescending(x => x.Name).ToList();
                    break;
                case "程序路径":
                    items = direction == ListSortDirection.Ascending ? 
                        items.OrderBy(x => x.Path).ToList() : 
                        items.OrderByDescending(x => x.Path).ToList();
                    break;
                case "添加时间":
                    items = direction == ListSortDirection.Ascending ? 
                        items.OrderBy(x => x.DateAdded).ToList() : 
                        items.OrderByDescending(x => x.DateAdded).ToList();
                    break;
            }

            StartupItems.Clear();
            foreach (var item in items)
            {
                StartupItems.Add(item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
