using ReactiveUI;

namespace Shavkat_grabber.Models;

public class ProductChecked : ReactiveObject
{
    public bool IsChecked
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public Product Good { get; set; }

    public ProductChecked(Product good)
    {
        Good = good;
    }
}
