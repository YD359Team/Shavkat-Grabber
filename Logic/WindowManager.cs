using System;
using System.Threading.Tasks;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Views;

namespace Shavkat_grabber.Logic;

public class WindowManager
{
    private MainWindow _mainWindow;
    public MainWindow MainWindow => _mainWindow;

    public WindowManager(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task ShowError(Exception ex)
    {
        ErrorWindow wnd = new ErrorWindow(ex.GetMessage());
        await wnd.ShowDialog(_mainWindow);
    }
}
