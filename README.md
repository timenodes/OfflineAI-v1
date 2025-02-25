> OfflineAI 基于C#

# 相关依赖
> OllamaSharpe：实现启用本地Ollama服务
> 
> Markdig.wpf :实现Markdown格式化输出功能。
> 
> Microsoft.Xaml.Behaviors.Wpf ：解决部分不能进行命令绑定的控件实现命令绑定功能。

# UI界面
# OfflineAI<img width="1269" alt="UI界面" src="https://github.com/user-attachments/assets/08b96450-c417-411c-83b0-30d2de64feb1" />

# 功能
> 1、实现AI聊天功能
> 
> 2、模型选择切换。
> 
> 3、聊天记录查看（未实现基于记录上下文回答问题）。
> 待完善。

# 说明
> 项目使用自己理解的MVVM模式、将程序结构通过目录的形式分为Modles、Views、ViewModels的目录进行分类。
> 
> 主要功能在viewmodes中编写view相关的功能，通过属性、命令绑定的方式显示交互。
> 
> 以上仅是个人理解，根据自己的想法、参考别的webUI界面编写的一个AI界面小案例，后面有时间会增加更多的功能。
