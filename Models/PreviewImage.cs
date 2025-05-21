using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Shavkat_grabber.Models;

/// <summary>
/// Превью изображения товара в окне постинга
/// </summary>
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
