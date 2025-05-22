using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Models;
using Shavkat_grabber.Views;
using Shavkat_grabber.Views.Dialogs;

namespace Shavkat_grabber.Logic;

public class WindowManager
{
    private MainWindow _mainWindow;
    public MainWindow MainWindow => _mainWindow;

    private TopLevel? _topLevel;
    public TopLevel TopLevel => _topLevel ?? (_topLevel = TopLevel.GetTopLevel(MainWindow));

    private readonly FilePickerFileType[] fileTypesCsv = [new("CSV") { Patterns = ["*.csv"] }];
    private readonly FilePickerFileType[] fileTypesXlsx = [new("XLSX") { Patterns = ["*.xlsx"] }];
    private readonly FilePickerFileType[] fileTypesPng = [new("PNG") { Patterns = ["*.png"] }];

    public WindowManager(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task ShowError(Exception ex)
    {
        ErrorWindow wnd = new ErrorWindow(ex.GetMessage());
        await wnd.ShowDialog(_mainWindow);
    }

    public async Task<string?> SaveFileDialog(SaveFileFormats format)
    {
        var options = new FilePickerSaveOptions();
        if (format == SaveFileFormats.CSV)
        {
            options.FileTypeChoices = fileTypesCsv;
        }
        else if (format == SaveFileFormats.XLSX)
        {
            options.FileTypeChoices = fileTypesXlsx;
        }
        else if (format == SaveFileFormats.PNG)
        {
            options.FileTypeChoices = fileTypesPng;
        }
        else
        {
            throw new NotImplementedException();
        }

        using var file = await TopLevel.StorageProvider.SaveFilePickerAsync(options);
        if (file is null)
            return null;

        return file.Path.AbsolutePath;
    }

    public async Task<string> Question(string question, string[] answers)
    {
        QuestionWindow wnd = new QuestionWindow();
        wnd.DataContext = new Question { QuestionText = question, Answers = answers };
        return await wnd.ShowDialog<string>(_mainWindow);
    }

    public enum SaveFileFormats
    {
        CSV,
        XLSX,
        PNG,
    }
}
