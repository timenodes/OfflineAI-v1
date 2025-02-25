using Markdig.Wpf;
using OfflineAI.Commands;
using OfflineAI.Services;
using OfflineAI.Sevices;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

namespace OfflineAI.ViewModels
{
    /// <summary>
    /// 描述：用户聊天视图模型：
    /// 作者：吾与谁归
    /// 时间: 2025年2月19日
    /// 更新：
    ///    1、 2025-02-19：添加AI聊天功能，输出问题及结果到UI,并使用Markdown相关的库做简单渲染。
    ///    2、 2025-02-20：优化了构造函数，使用无参构造，方便在设计器中直接绑定数据上下文（感觉）。
    ///    3、 2025-02-20：滚轮滑动显示内容,提交问题后滚动显示内容，鼠标右键点击内容停止继续滚动，回答结束停止滚动。
    ///    4、 2025-02-24：添加聊天记录保存功能。
    ///    5、 2025-02-24：添加聊天记录加载功能，通过点击记录列表显示。
    /// </summary>
    public class UserChatViewModel:PropertyChangedBase
    {
        #region 字段、属性、集合、命令

        #region 字段
        private bool _isAutoScrolling = false;      //是否自动滚动
        private string _currentInputText;           //当前输入文本
        private string _messageContent;             //消息内容
        private string _directory;                  //目录
        private string _fileName;                   //文件名
       
        private MarkdownViewer _markdownViewer;                 //MarkdownViewer控件
        private ScrollViewer _scrollViewer;                     //ScrollViewer滑动控件
        private StringBuilder _message = new StringBuilder();   //消息字符串拼接
        private CancellationToken cancellationToken;            //异步线程取消标记
        
        private FileOperation _fileIO;              //文件IO
        private ShareOllamaObject _ollama;          //Ollama 对象实例
        private string _submitButtonName;
        #endregion

        #region 属性
        /// <summary>
        /// 提交按钮名称
        /// </summary>
        public string SubmitButtonName
        {
            get => _submitButtonName;
            set
            {
                if (_submitButtonName != value)
                {
                    _submitButtonName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string? MessageContent
        {
            get => _messageContent;
            set
            {
                _messageContent = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 当前输入文本
        /// </summary>
        public string CurrentInputText
        {
            get => _currentInputText;
            set
            {
                if (_currentInputText != value)
                {
                    _currentInputText = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 共享Ollama对象 
        /// </summary>
        public ShareOllamaObject Ollama 
        {
            get => _ollama;
            set
            {
                if (_ollama != value)
                {
                    _ollama = value;
                    OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 自动滚动消息
        /// </summary>
        public bool IsAutoScrolling
        {
            get => _isAutoScrolling;
            set
            {
                if (_isAutoScrolling != value)
                {
                    _isAutoScrolling = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region 集合

        #endregion

        #region 命令
        /// <summary>
        /// 展开功能菜单命令
        /// </summary>
        public ICommand LoadFileCommand { get; set; }

        /// <summary>
        /// 提交命令
        /// </summary>
        public ICommand SubmiQuestionCommand { get; set; }

        /// <summary>
        /// 鼠标滚动
        /// </summary>
        public ICommand MouseWheelCommand { get; set; }

        /// <summary>
        /// 鼠标按下
        /// </summary>
        public ICommand MouseDownCommand { get; set; }

        /// <summary>
        /// Markdown对象命令
        /// </summary>
        public ICommand MarkdownOBJCommand { get; set; }
        
        /// <summary>
        /// 滑动条加载
        /// </summary>
        public ICommand ScrollLoadedCommand { get; set; }
        #endregion

        #endregion

        #region 构造函数
        public UserChatViewModel()
        {
            Initialize();
        }
        #endregion

        #region 初始化方法
        /// <summary>
        /// 初始化方法
        /// </summary>
        public void Initialize()
        {
            //文件加载
            LoadFileCommand = new ParameterCommand(LoadFileTrigger);
            MouseWheelCommand = new EventsCommand<MouseWheelEventArgs>(MouseWheelTrigger);
            MouseDownCommand = new EventsCommand<MouseButtonEventArgs>(MouseDownTrigger);
            MarkdownOBJCommand = new EventsCommand<object>(MarkdownOBJTrigger);
            SubmiQuestionCommand = new ParameterlessCommand(SubmitQuestionTrigger);
            ScrollLoadedCommand = new EventsCommand<RoutedEventArgs>(ScrollLoadedTrigger);

            //
            SubmitButtonName = "提交";

            //日志记录
            _directory = $"{Environment.CurrentDirectory}\\Record\\";
            _fileName = $"{_directory}\\{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            _fileIO = new FileOperation($"{_fileName}");

            //
        }
        #endregion

        #region 命令方法

        /// <summary>
        /// 加载文件
        /// </summary>
        private void LoadFileTrigger(object obj)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Multiselect = true;
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                string[] files = openFile.FileNames;
                if (files.Count() > 1)
                {
                    foreach (var item in files)
                    {
                        Debug.WriteLine(item);
                    }
                }
                else
                {
                    Debug.WriteLine(openFile.FileName);
                }
            }
        }

        /// <summary>
        /// 提交:  提交问题到AI并获取返回结果
        /// </summary>
        private async void SubmitQuestionTrigger()
        {
            _ = Task.Delay(1);
            string input = CurrentInputText;
            try
            {
                if (!SubmintChecked(input)) return; 
                SubmitButtonName = "停止";
                _message.Clear();
                _isAutoScrolling = true;
                AppendText($"##{Environment.NewLine}");
                AppendText($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff")}]{Environment.NewLine}");
                AppendText($"## 【User】{Environment.NewLine}");
                AppendText($">{input}{Environment.NewLine}");
                AppendText($"{Environment.NewLine}");
                AppendText($"## 【AI】{Environment.NewLine}");
                await foreach (var answerToken in Ollama.Chat.SendAsync(input))
                {
                    AppendText(answerToken);
                    await Task.Delay(20);
                    if (_isAutoScrolling) _scrollViewer.ScrollToEnd();//是否自动滚动
                }
                AppendText($"{Environment.NewLine}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                AppendText($"Error: {ex.Message}");
                AppendText($"{Environment.NewLine}{Environment.NewLine}");
            }
            //回答完成
            _fileIO.WriteTxt($"{_fileName}", _message.ToString());
            CurrentInputText = string.Empty;
            _isAutoScrolling = false;
            SubmitButtonName = "提交";
        }

        /// <summary>
        /// 鼠标滚动上下滑动
        /// </summary>
        private void MouseWheelTrigger(MouseWheelEventArgs e)
        {
            try
            {
                // 获取 ScrollViewer 对象
                if (e.Source is FrameworkElement element && element.Parent is ScrollViewer scrollViewer)
                {
                    // 获取当前的垂直偏移量
                    double currentOffset = scrollViewer.VerticalOffset;
                    if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToVerticalOffset(currentOffset - e.Delta);
                    }
                    else
                    {
                        scrollViewer.ScrollToVerticalOffset(currentOffset - e.Delta);
                    }
                    // 标记事件已处理，防止默认滚动行为
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        /// <summary>
        /// Markdown中鼠标按下
        /// </summary>
        private void MouseDownTrigger(MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                IsAutoScrolling = false;
                Debug.Print("Mouse Down...");
            }
        }

        /// <summary>
        /// 滚动栏触发
        /// </summary>
        private void ScrollLoadedTrigger(RoutedEventArgs args)
        {
            if (args.Source is ScrollViewer scrollView )
            {
                _scrollViewer = scrollView;
                Debug.Print("Scroll loaded...");
            }
        }

        /// <summary>
        /// Markdown控件对象更新触发
        /// </summary>
        private void MarkdownOBJTrigger(object obj)
        {
            if (_markdownViewer != null) return;
            if (obj is MarkdownViewer markdownViewer)
            {
                _markdownViewer = markdownViewer;
                _markdownViewer.Markdown = "";
            }
        }
        #endregion

        #region 其他方法

        /// <summary>
        /// 输出文本
        /// </summary>
        public void AppendText(string newText)
        {
            Debug.Print(newText);
            _markdownViewer.Markdown += newText;
            _message.Append(newText);
        }

        /// <summary>
        /// 提交校验
        /// </summary>
        private bool SubmintChecked(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Length<2) return false;
            if (input.Equals("停止")) return false;
            return true;
        }
        #endregion

        #region 回调方法

        /// <summary>
        ///  加载聊天记录回调
        /// </summary>
        public void LoadChatRecordCallback(string path)
        {
            Debug.Print(path);
            _scrollViewer.ScrollToTop();
            _markdownViewer.Markdown = _fileIO. ReadTxt(path);
        }
        #endregion

    }
}
