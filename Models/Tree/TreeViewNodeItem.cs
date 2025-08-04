namespace Shavkat_grabber.Models.Tree;

public class TreeViewNodeItem : TreeViewNode
{
    public bool IsChecked { get; set; }

    public TreeViewNodeItem(int id, string title)
    {
        Id = id;
        Title = title;
    }
}
