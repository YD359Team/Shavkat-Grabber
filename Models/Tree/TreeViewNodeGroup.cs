using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Shavkat_grabber.Models.Tree;

public class TreeViewNodeGroup : TreeViewNode
{
    public int Id { get; set; }
    public bool IsExpanded { get; set; } = true;
    public ObservableCollection<TreeViewNodeItem> Children { get; set; }

    public TreeViewNodeGroup(string title, params TreeViewNodeItem[] children)
    {
        Title = title;
        Children = new ObservableCollection<TreeViewNodeItem>(children);
    }

    public TreeViewNodeGroup(int id, string title, params TreeViewNodeItem[] children)
    {
        Id = id;
        Title = title;
        Children = new ObservableCollection<TreeViewNodeItem>(children);
    }
}
