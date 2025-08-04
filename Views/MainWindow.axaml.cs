using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Shavkat_grabber.Views;

public partial class MainWindow : Window
{
    private INotificationManager manager = new WindowNotificationManager();

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        // Инициализация WindowNotificationManager
        manager = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight, // Положение уведомления
            MaxItems = 6, // Максимальное количество одновременно отображаемых уведомлений
        };
    }

    /// <summary>
    /// Гарантирует прокрутку ListBox до переданного элемента
    /// </summary>
    /// <param name="item"></param>
    public void ScrollListBox(object item)
    {
        ListboxLog.SelectedItem = item;
        ListboxLog.ScrollIntoView(item);
    }

    public void ShowNotification(string title, string msg, NotificationType type)
    {
        manager.Show(new Notification(title, msg, type, TimeSpan.FromSeconds(5)));
    }
}
