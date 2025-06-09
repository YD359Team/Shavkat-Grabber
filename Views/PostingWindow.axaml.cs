using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Shavkat_grabber.ViewModels;

namespace Shavkat_grabber.Views;

public partial class PostingWindow : Window
{
    private PostingWindowViewModel _vm;

    public PostingWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        btnBold.Click += BtnBoldOnClick;
        btnItalic.Click += BtnItalicOnClick;
        btnLink.Click += BtnLinkOnClick;
    }

    private void BtnLinkOnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbPostTg.SelectedText))
            return;

        int start = tbPostTg.SelectionStart;
        int end = tbPostTg.SelectionEnd;
        _vm.MorphText(start, end, "[", "]()");
        tbPostTg.SelectionStart = start + 1;
        tbPostTg.SelectionEnd = end + 1;
    }

    private void BtnItalicOnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbPostTg.SelectedText))
            return;

        int start = tbPostTg.SelectionStart;
        int end = tbPostTg.SelectionEnd;
        _vm.MorphText(start, end, "_");
        tbPostTg.SelectionStart = start + 1;
        tbPostTg.SelectionEnd = end + 1;
    }

    private void BtnBoldOnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbPostTg.SelectedText))
            return;

        int start = tbPostTg.SelectionStart;
        int end = tbPostTg.SelectionEnd;
        _vm.MorphText(start, end, "*");
        tbPostTg.SelectionStart = start + 1;
        tbPostTg.SelectionEnd = end + 1;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _vm = (PostingWindowViewModel)DataContext!;
        tbPostTg.PastingFromClipboard += tbPostTgOnPastingFromClipboard;
    }

    private async void tbPostTgOnPastingFromClipboard(object? sender, RoutedEventArgs e)
    {
        var clipboard = this.Clipboard;
        var dataObject =
            await clipboard.GetDataAsync("image/png")
            ?? await clipboard.GetDataAsync("image/jpeg")
            ?? await clipboard.GetDataAsync("PNG")
            ?? await clipboard.GetDataAsync("Bitmap");
        if (dataObject is null)
            return;

        _vm.PastImage(dataObject);
        e.Handled = true;
    }
}
