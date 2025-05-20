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
        if (string.IsNullOrWhiteSpace(tbPost.SelectedText))
            return;

        int start = tbPost.SelectionStart;
        int end = tbPost.SelectionEnd;
        _vm.MorphText(start, end, "[", "]()");
        tbPost.SelectionStart = start + 1;
        tbPost.SelectionEnd = end + 1;
    }

    private void BtnItalicOnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbPost.SelectedText))
            return;

        int start = tbPost.SelectionStart;
        int end = tbPost.SelectionEnd;
        _vm.MorphText(start, end, "_");
        tbPost.SelectionStart = start + 1;
        tbPost.SelectionEnd = end + 1;
    }

    private void BtnBoldOnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbPost.SelectedText))
            return;

        int start = tbPost.SelectionStart;
        int end = tbPost.SelectionEnd;
        _vm.MorphText(start, end, "*");
        tbPost.SelectionStart = start + 1;
        tbPost.SelectionEnd = end + 1;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _vm = (PostingWindowViewModel)DataContext!;
        tbPost.PastingFromClipboard += TbPostOnPastingFromClipboard;
    }

    private async void TbPostOnPastingFromClipboard(object? sender, RoutedEventArgs e)
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
