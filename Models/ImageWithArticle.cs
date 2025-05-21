using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Shavkat_grabber.Models;

/// <summary>
/// Картинка с артикулом. Используется для коллажа
/// </summary>
public class ImageWithArticle : ReactiveObject
{
    public Bitmap Image { get; set; }
    public string Article { get; set; }

    public ImageWithArticle(string article, Bitmap image)
    {
        Article = article;
        Image = image;
    }
}
