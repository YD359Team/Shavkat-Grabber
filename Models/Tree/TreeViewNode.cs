using ReactiveUI;

namespace Shavkat_grabber.Models.Tree;

public abstract class TreeViewNode : ReactiveObject
{
    public int Id { get; set; }
    public string Title { get; set; }
}
