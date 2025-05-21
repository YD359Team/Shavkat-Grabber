namespace Shavkat_grabber.Models;

public class TreeViewNodeItem : TreeViewNode
{
    public bool IsChecked { get; set; }
    
    public TreeViewNodeItem(string title)
    {
        Title = title;
    }
}