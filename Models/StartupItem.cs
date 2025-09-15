using System;
using System.ComponentModel;
using System.IO;

namespace StartupManager.Models
{
    /// <summary>
    /// 启动项信息模型
    /// </summary>
    public class StartupItem : INotifyPropertyChanged
    {
        private string _name;
        private string _path;
        private string _arguments;
        private bool _isEnabled;
        private DateTime _dateAdded;

        /// <summary>
        /// 程序名称
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// 程序路径
        /// </summary>
        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        /// <summary>
        /// 启动参数
        /// </summary>
        public string Arguments
        {
            get => _arguments;
            set
            {
                _arguments = value;
                OnPropertyChanged(nameof(Arguments));
            }
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        /// <summary>
        /// 添加日期
        /// </summary>
        public DateTime DateAdded
        {
            get => _dateAdded;
            set
            {
                _dateAdded = value;
                OnPropertyChanged(nameof(DateAdded));
            }
        }

        /// <summary>
        /// 快捷方式文件路径
        /// </summary>
        public string ShortcutPath { get; set; }

        /// <summary>
        /// 程序图标
        /// </summary>
        public string IconPath
        {
            get
            {
                if (File.Exists(Path))
                {
                    return Path;
                }
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
