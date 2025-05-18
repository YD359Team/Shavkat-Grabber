using System.Collections.Generic;
using ReactiveUI;

namespace Shavkat_grabber.Models;

public abstract class TreeViewNode : ReactiveObject
{
    public string Title { get; set; }
}

public class TreeViewNodeGroup :  TreeViewNode
{
    public bool IsExpanded { get; set; } = true;
    public IReadOnlyList<TreeViewNodeItem> Children { get; set; }

    public TreeViewNodeGroup(string title, params TreeViewNodeItem[] children)
    {
        Title = title;
        Children = new List<TreeViewNodeItem>(children).AsReadOnly();
    }
}

public class TreeViewNodeItem : TreeViewNode
{
    public bool IsChecked { get; set; }
    
    public TreeViewNodeItem(string title)
    {
        Title = title;
    }
}