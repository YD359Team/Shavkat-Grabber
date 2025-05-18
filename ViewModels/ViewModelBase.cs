using ReactiveUI;
using Shavkat_grabber.Logic;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.ViewModels;

public class ViewModelBase : ReactiveObject { }

public class ChildViewModel : ViewModelBase
{
    protected WindowManager Manager { get; }
    protected AppSettings Settings { get; }

    public ChildViewModel(WindowManager windowManager, AppSettings settings)
    {
        Manager = windowManager;
        Settings = settings;
    }
}
