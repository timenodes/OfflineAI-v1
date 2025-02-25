using Microsoft.Xaml.Behaviors;
using System;
using System.Windows.Input;

/// <summary>
/// 事件命令：
///   有些控件的无法绑定命令，但是想要实现命令绑定功能，可通过创建该命令实现。
///   需要引用Microsoft.Xaml.Behaviors.Wpf组合实现。
/// </summary>
public class EventsCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;
    
    public EventsCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute?.Invoke((T)parameter) ?? true;
    }

    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}