using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Shavkat_grabber.Views.Dialogs;

public partial class QuestionWindow : Window
{
    public QuestionWindow()
    {
        InitializeComponent();
        ListAnswers.SelectionChanged += ListAnswersOnSelectionChanged;
    }

    private void ListAnswersOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count < 1)
            return;

        string answer = e.AddedItems[0].ToString();
        Close(answer);
    }
}
