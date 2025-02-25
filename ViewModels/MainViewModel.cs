using OfflineAI.Sevices;
using OfflineAI.Commands;
using OfflineAI.Views;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.IO;
using OfflineAI.Services;
using OfflineAI.Models;

namespace OfflineAI.ViewModels
{
    /// <summary>
    /// 主窗体视图模型：
    /// 作者：吾与谁归
    /// 时间：2025年02月17日（首次创建时间）
    /// 更新: 
    ///     1、2025-02-17：添加折叠栏展开|折叠功能。
    ///     2、2025-02-17：视图切换功能 1）系统设置 2) 聊天
    ///     3、2025-02-18：关闭窗体时提示是否关闭，释放相关资源。
    ///     4、2025-02-19：添加首页功能，和修改新聊天功能。点击新聊天会创建新的会话（Chat）。
    ///     5、2025-02-20：窗体加载时传递Ollama对象。
    ///     6、2025-02-24：添加了窗体加载时，加载聊天记录的功能。
    /// </summary>
    public class MainViewModel : PropertyChangedBase
    {
        #region 字段、属性、集合、命令
       
        #region 字段
        private UserControl _currentView;           //当前视图
        private ShareOllamaObject _ollamaService;   //共享Ollama服务对象
        private string _selectedModel;              //选择的模型
        private ObservableCollection<string> _modelListCollection;  //模型列表
        private int _expandedBarWidth = 50;         //折叠栏宽度
        private string _directory;                  //目录
        private string _fileName;                   //文件
        private ObservableCollection<ChatRecordModel> _chatRecordCollection;
        public event Action<string> LoadChatRecordEventHandler;
        #endregion

        #region 属性
        /// <summary>
        /// 当前显示视图
        /// </summary>
        public UserControl CurrentView { 
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }
        public ShareOllamaObject OllamaService
        {
            get => _ollamaService;
            set
            {
                if (_ollamaService != value)
                {
                    _ollamaService = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SelectedModel 
        { 
            get => _selectedModel;
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    OllamaService.Ollama.SelectedModel = value;
                    OllamaService.Chat.Model = value;
                    OnPropertyChanged();
                }
            }
        }
        public int ExpandedBarWidth
        {
            get => _expandedBarWidth;
            set
            {
                if (_expandedBarWidth != value)
                {
                    _expandedBarWidth = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region 集合
        /// <summary>
        /// 视图集合，保存视图
        /// </summary>
        public ObservableCollection<UserControl> ViewCollection { get; set; }
        public ObservableCollection<string> ModelListCollection
        {
            get => _modelListCollection;
            set
            {
                if (_modelListCollection != value)
                {
                    _modelListCollection = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<ChatRecordModel> ChatRecordCollection
        {
            get => _chatRecordCollection;
            set
            {
                if (_chatRecordCollection != value)
                {
                    _chatRecordCollection = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region 命令
        /// <summary>
        /// 展开功能菜单命令
        /// </summary>
        public ICommand ExpandedMenuCommand { get; set; }
        /// <summary>
        /// 折叠功能菜单命令
        /// </summary>
        public ICommand CollapsedMenuCommand { get; set; }
        /// <summary>
        /// 切换视图命令
        /// </summary>
        public ICommand SwitchViewCommand { get; set; }
        /// <summary>
        /// 窗体关闭命令
        /// </summary>
        public ICommand ClosingWindowCommand {  get; set; }
        /// <summary>
        /// 窗体加载命令
        /// </summary>
        public ICommand LoadedWindowCommand { get; set; }
        
        public ICommand ChatRecordMouseDownCommand { get; set; }
        #endregion
        #endregion

        #region 构造函数
        public MainViewModel()
        {
            Initialize();
        }
        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Initialize()
        {
            //初始化Ollama
            _ollamaService = new ShareOllamaObject();
            ModelListCollection = _ollamaService.ModelList;
            SelectedModel = _ollamaService.SelectModel;

            //创建命令
            SwitchViewCommand = new ParameterCommand(SwitchViewTrigger);

            LoadedWindowCommand = new EventsCommand<object>(LoadedWindowTrigger);
            CollapsedMenuCommand = new EventsCommand<object>(CollapsedMenuTrigger);
            ExpandedMenuCommand = new EventsCommand<object>(ExpandedMenuTrigger);
            ClosingWindowCommand = new EventsCommand<object>(ClosingWindowTrigger);
            ChatRecordMouseDownCommand = new EventsCommand<ChatRecordModel>(ChatRecordMouseDownTrigger);
            ViewCollection = new ObservableCollection<UserControl>();

            //添加视图到集合
            ViewCollection.Add(new SystemSettingView());
            ViewCollection.Add(new UserChatView());
            //默认显示窗体
            CurrentView = ViewCollection[1];
            //折叠栏折叠状态
            ExpandedBarWidth = 25;
            //加载聊天记录
            LoadChatRecord();
        }

        #endregion

        #region 命令方法
        /// <summary>
        /// 聊天记录鼠标按下
        /// </summary>
        private void ChatRecordMouseDownTrigger(ChatRecordModel obj)
        {
            Debug.Print(obj.ToString());
            OnLoadChatRecordCallBack(obj.FullName.ToString());
        }

        /// <summary>
        /// 触发主视图窗体加载方法
        /// </summary>
        private void LoadedWindowTrigger(object sender)
        {
            Debug.Print(sender?.ToString());
            var userView = ViewCollection.FirstOrDefault(obj => obj is UserChatView) as UserChatView;
            userView.UserWindow.Ollama = _ollamaService;
            LoadChatRecordEventHandler += userView.UserWindow.LoadChatRecordCallback;
        }
       
        /// <summary>
        /// 触发关闭窗体方法
        /// </summary>
        private void ClosingWindowTrigger(object obj)
        {
            if (obj is CancelEventArgs cancelEventArgs)
            {
                if (MessageBox.Show("确定要关闭程序吗？", "确认关闭", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    cancelEventArgs.Cancel = true; // 取消关闭
                }
                else
                {
                    ClearingResources();
                }
            }
        }

        /// <summary>
        /// 视图切换命令触发的方法
        /// </summary>
        private void SwitchViewTrigger(object obj)
        {
            Debug.WriteLine(obj.ToString());
            switch (obj.ToString())
            {
                case "SystemSettingView":
                    CurrentView = ViewCollection[0];
                    break;
                case "UserChatView":
                    CurrentView = ViewCollection[1];
                    break;
                case "NewUserChatView":
                    UserChatView newChatView = new UserChatView();
                    OllamaService.ReCreateChat();
                    newChatView.UserWindow.Ollama = OllamaService;
                    ViewCollection[1] = newChatView;
                    CurrentView = newChatView;
                    break;
            }
        }

        /// <summary>
        /// 折叠菜单触发方法
        /// </summary>
        private void CollapsedMenuTrigger(object e)
        {
            ExpandedBarWidth = 25;
            Debug.WriteLine("折叠");
        }
        /// <summary>
        /// 展开菜单触发方法
        /// </summary>
        private void ExpandedMenuTrigger(object e)
        {
            ExpandedBarWidth = 250;
            Debug.WriteLine("展开");
        }
        #endregion

        #region 其他方法
        /// <summary>
        /// 加载聊天记录
        /// </summary>
        private void LoadChatRecord()
        {
            _directory = $"{Environment.CurrentDirectory}\\Record";
            string[] files = FileOperation.GetFiles(_directory);
            ObservableCollection<ChatRecordModel> records = new ObservableCollection<ChatRecordModel>();
            string name = string.Empty;
            string data = string.Empty;
            foreach (var item in files)
            {
                name = Path.GetFileNameWithoutExtension(item);
                data = File.ReadAllLines(item)[3];
                if (data.Trim().Length > 1 )
                {
                    records.Add(new ChatRecordModel(records.Count, name, name, item, data.Substring(1)));
                }
            }
            ChatRecordCollection = records;
        }

        /// <summary>
        /// 触发事件：加载聊天记录回调
        /// </summary>
        private void OnLoadChatRecordCallBack(object sender)
        {
            LoadChatRecordEventHandler.Invoke(sender.ToString());
        }

        /// <summary>
        /// 释放资源：窗体关闭时触发
        /// </summary>
        private void ClearingResources()
        {
            //ProcessService.GetPIDAndCloseByPort(11434);
        }
        #endregion
    }
}
