using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Shavkat_grabber.Logic;

namespace Shavkat_grabber.Views.Dialogs;

public partial class QuestionWindow : Window
{
    public QuestionWindow(string title, string[] btns)
    {
        InitializeComponent();
        tblTitle.Text = title;
        if (btns.Contains("Да"))
        {
            btnYes.Click += BtnsOnClick;
        }
        else
        {
            btnYes.IsVisible = false;
        }

        if (btns.Contains("Ок"))
        {
            btnOk.Click += BtnsOnClick;
        }
        else
        {
            btnOk.IsVisible = false;
        }

        if (btns.Contains("Нет"))
        {
            btnNo.Click += BtnsOnClick;
        }
        else
        {
            btnNo.IsVisible = false;
        }

        if (btns.Contains("Отмена"))
        {
            btnCancel.Click += BtnsOnClick;
        }
        else
        {
            btnCancel.IsVisible = false;
        }
    }

    private void BtnsOnClick(object? sender, RoutedEventArgs e)
    {
        Button btn = (Button)sender;
        if (btn == btnYes)
            Close(DialogResultButtons.Yes);
        if (btn == btnOk)
            Close(DialogResultButtons.Ok);
        if (btn == btnNo)
            Close(DialogResultButtons.No);
        if (btn == btnCancel)
            Close(DialogResultButtons.Cancel);
    }
}
