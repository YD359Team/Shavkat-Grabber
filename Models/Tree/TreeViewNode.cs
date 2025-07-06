using ReactiveUI;

namespace Shavkat_grabber.Models.Tree;

public abstract class TreeViewNode : ReactiveObject
{
    public string Title { get; set; }
}
