using System.Collections.Generic;

namespace Shavkat_grabber.Models.Tree;

public class TreeViewNodeGroup : TreeViewNode
{
    public bool IsExpanded { get; set; } = true;
    public IReadOnlyList<TreeViewNodeItem> Children { get; set; }

    public TreeViewNodeGroup(string title, params TreeViewNodeItem[] children)
    {
        Title = title;
        Children = new List<TreeViewNodeItem>(children).AsReadOnly();
    }
}
