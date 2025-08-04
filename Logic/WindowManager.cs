using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Logic.Pattern;
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

    private readonly DialogResultButtons[] _buttons =
    [
        DialogResultButtons.Cancel,
        DialogResultButtons.Ok,
        DialogResultButtons.Yes,
        DialogResultButtons.No,
    ];

    private string Button2Text(DialogResultButtons btn)
    {
        return btn switch
        {
            DialogResultButtons.Ok => "Ок",
            DialogResultButtons.Cancel => "Отмена",
            DialogResultButtons.Yes => "Да",
            DialogResultButtons.No => "Нет",
            _ => throw new ArgumentOutOfRangeException(nameof(btn), btn, null),
        };
    }

    public async Task<DialogResultButtons> Question(string question, DialogResultButtons buttons)
    {
        string[] btnsText = _buttons.Where(x => buttons.HasFlag(x)).Select(Button2Text).ToArray();
        QuestionWindow wnd = new(question, btnsText);
        return await wnd.ShowDialog<DialogResultButtons>(_mainWindow);
    }

    public async Task<Result<string>> ShowEditForm(EditMode mode, string content = null)
    {
        EditTextWindow wnd = mode switch
        {
            EditMode.Add => new("Создание", content),
            EditMode.Edit => new("Редактирование", content),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
        };

        return await wnd.ShowDialog<Result<string>>(_mainWindow);
    }

    public enum FileFormats
    {
        Csv,
        Xlsx,
        Png,
        Json,
    }

    public enum EditMode
    {
        Add,
        Edit,
    }
}

[Flags]
public enum DialogResultButtons
{
    Cancel = 1,
    Ok = Cancel << 1,
    Yes = Cancel << 2,
    No = Cancel << 3,
}
