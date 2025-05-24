using System;
using System.Linq;
using System.Net;
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
    private readonly FilePickerFileType[] fileTypesJson = [new("JSON") { Patterns = ["*.json"] }];

    public WindowManager(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task ShowError(Exception ex)
    {
        ErrorWindow wnd = new ErrorWindow(ex.GetMessage());
        await wnd.ShowDialog(_mainWindow);
    }

    public async Task<string?> SaveFileDialog(FileFormats format)
    {
        var options = new FilePickerSaveOptions();
        if (format == FileFormats.Csv)
        {
            options.FileTypeChoices = fileTypesCsv;
        }
        else if (format == FileFormats.Xlsx)
        {
            options.FileTypeChoices = fileTypesXlsx;
        }
        else if (format == FileFormats.Png)
        {
            options.FileTypeChoices = fileTypesPng;
        }
        else if (format == FileFormats.Json)
        {
            options.FileTypeChoices = fileTypesJson;
        }
        else
        {
            throw new NotImplementedException();
        }

        using var file = await TopLevel.StorageProvider.SaveFilePickerAsync(options);
        if (file is null)
            return null;

        return WebUtility.UrlDecode(file.Path.AbsolutePath);
    }

    public async Task<string?> OpenFileDialog(FileFormats format)
    {
        var options = new FilePickerOpenOptions();
        if (format == FileFormats.Csv)
        {
            options.FileTypeFilter = fileTypesCsv;
        }
        else if (format == FileFormats.Xlsx)
        {
            options.FileTypeFilter = fileTypesXlsx;
        }
        else if (format == FileFormats.Png)
        {
            options.FileTypeFilter = fileTypesPng;
        }
        else if (format == FileFormats.Json)
        {
            options.FileTypeFilter = fileTypesJson;
        }
        else
        {
            throw new NotImplementedException();
        }

        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(options);
        if (files is null || files.Count < 1)
            return null;

        return WebUtility.UrlDecode(files.First().Path.AbsolutePath);
    }

    public async Task<string> Question(string question, string[] answers)
    {
        QuestionWindow wnd = new QuestionWindow();
        wnd.DataContext = new Question(question, answers);
        return await wnd.ShowDialog<string>(_mainWindow);
    }

    public enum FileFormats
    {
        Csv,
        Xlsx,
        Png,
        Json,
    }
}
