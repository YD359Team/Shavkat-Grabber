using Shavkat_grabber.Logic;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.ViewModels;

/// <summary>
/// Родительская VM для всех дочерних <see cref="Views.MainWindow"/> окон
/// </summary>
public abstract class ChildViewModel : ViewModelBase
{
    protected FileSystemManager FsManager { get; }
    protected WindowManager WinManager { get; }
    protected AppSettings Settings { get; }

    protected ChildViewModel(
        FileSystemManager fsManager,
        WindowManager winManager,
        AppSettings settings
    )
    {
        FsManager = fsManager;
        WinManager = winManager;
        Settings = settings;
    }
}
