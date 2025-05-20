using Shavkat_grabber.Logic;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.ViewModels;

public class HelpWindowViewModel : ChildViewModel
{
    public HelpWindowViewModel(
        FileSystemManager fsManager,
        WindowManager winManager,
        AppSettings settings
    )
        : base(fsManager, winManager, settings) { }
}
