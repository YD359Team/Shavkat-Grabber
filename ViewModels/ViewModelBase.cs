using ReactiveUI;
using Shavkat_grabber.Logic;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.ViewModels;

public class ViewModelBase : ReactiveObject { }

public class ChildViewModel : ViewModelBase
{
    protected FileSystemManager FsManager { get; }
    protected WindowManager WinManager { get; }
    protected AppSettings Settings { get; }

    public ChildViewModel(
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
