using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Shavkat_grabber.Models;

public class PreviewImage : ReactiveObject
{
    public int Id { get; set; }
    public Bitmap Image { get; set; }

    public PreviewImage(int id, Bitmap image)
    {
        Id = id;
        Image = image;
    }
}
