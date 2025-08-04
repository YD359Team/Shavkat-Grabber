namespace Shavkat_grabber.Models;

/// <summary>
/// Товар
/// </summary>
public class Product
{
    public string Article { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string Price { get; set; }
    public string ImageUrl { get; set; }
    public string Marketplace { get; set; }

    public override string ToString()
    {
        return $"{Article} {Title} {Price} {Url}";
    }
}
