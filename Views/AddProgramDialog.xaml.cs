using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace StartupManager.Views
{
    /// <summary>
    /// AddProgramDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddProgramDialog : Window
    {
        public string ProgramName => TxtName.Text.Trim();
        public string ProgramPath => TxtPath.Text.Trim();
        public string Arguments => TxtArguments.Text.Trim();

        public AddProgramDialog()
        {
            InitializeComponent();
            TxtName.Focus();
        }

        /// <summary>
        /// 浏览按钮点击事件
        /// </summary>
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择要添加到启动项的程序",
                Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                TxtPath.Text = openFileDialog.FileName;
                
                // 如果名称为空，自动填充程序名称
                if (string.IsNullOrWhiteSpace(TxtName.Text))
                {
                    TxtName.Text = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                }
            }
        }

        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(ProgramName))
            {
                MessageBox.Show("请输入程序名称！", "输入错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ProgramPath))
            {
                MessageBox.Show("请选择程序文件！", "输入错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPath.Focus();
                return;
            }

            if (!File.Exists(ProgramPath))
            {
                MessageBox.Show("指定的程序文件不存在！", "文件错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                TxtPath.Focus();
                return;
            }

            // 检查文件扩展名
            var extension = Path.GetExtension(ProgramPath).ToLower();
            if (extension != ".exe" && extension != ".bat" && extension != ".cmd")
            {
                var result = MessageBox.Show(
                    "选择的文件不是常见的可执行文件类型，确定要继续吗？", 
                    "文件类型警告", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }


    }
}
