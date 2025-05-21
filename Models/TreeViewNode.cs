using ReactiveUI;

namespace Shavkat_grabber.Models;

public abstract class TreeViewNode : ReactiveObject
{
    public string Title { get; set; }
}
