using ReactiveUI;

namespace Shavkat_grabber.Models;

/// <summary>
/// Товар
/// </summary>
public class Good
{
    public string Article { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public string Price { get; set; }
    public string? OldPrice { get; set; }
    public string? Vendor { get; set; }
    public string ImageUrl { get; set; }
    public string? Rating { get; set; }

    public override string ToString()
    {
        return $"{Article} {Title} {Price} {Vendor} {Url}";
    }
}

public class GoodItem : ReactiveObject
{
    private bool _isChecked;
    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }
    public Good Good { get; set; }

    public GoodItem(Good good)
    {
        Good = good;
    }
}
