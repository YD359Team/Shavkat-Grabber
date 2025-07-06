using Shavkat_grabber.Logic;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.ViewModels;

public class DatabaseWindowViewModel : ChildViewModel
{
    public DatabaseWindowViewModel(
        FileSystemManager fsManager,
        WindowManager winManager,
        AppSettings settings
    )
        : base(fsManager, winManager, settings) { }
}
