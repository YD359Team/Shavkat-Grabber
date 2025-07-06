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
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _vm = (PostingWindowViewModel)DataContext!;
    }
}
