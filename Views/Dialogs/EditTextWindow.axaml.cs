using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Logic.Pattern;

namespace Shavkat_grabber.Views.Dialogs;

public partial class EditTextWindow : Window
{
    public EditTextWindow()
    {
        InitializeComponent();
    }

    public EditTextWindow(string title, string content)
    {
        InitializeComponent();
        btnSave.Click += (sender, e) => Close(tbText.Text.ToResult());
        btnCancel.Click += (sender, e) => Close(Result<string>.Fail());
        tblTitle.Text = title;
        tbText.Text = content;
    }
}
