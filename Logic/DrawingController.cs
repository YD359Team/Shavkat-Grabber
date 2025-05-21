using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using Shavkat_grabber.Models;
using SkiaSharp;

namespace Shavkat_grabber.Logic;

public class DrawingController
{
    public Bitmap CreateCollage(
        string market,
        List<ImageWithArticle> images,
        int collageWidth = 1200,
        int collageHeight = 800
    )
    {
        if (images == null || images.Count < 2 || images.Count > 5)
            throw new ArgumentException("Number of images should be between 2 and 5");

        var collage = new SKBitmap(collageWidth, collageHeight);
        using var canvas = new SKCanvas(collage);

        // Заливаем фон белым цветом
        canvas.Clear(SKColors.White);

        var layout = GetLayout(images.Count, collageWidth, collageHeight);

        for (int i = 0; i < images.Count; i++)
        {
            var image = images[i];
            var position = layout[i];

            var scaledBitmap = ScaleBitmap(
                AvaloniaToSkia(image.Image),
                (int)position.Width,
                (int)position.Height
            );

            canvas.DrawBitmap(scaledBitmap, position.Left, position.Top);

            // Добавляем текст с черным цветом на белом фоне
            DrawText(
                canvas,
                $"{market}: {image.Article}",
                position.Left + position.Width / 2,
                position.Top + position.Height - 20,
                (int)position.Width
            );
        }

        return SkiaToAvalonia(collage);
    }

    private SKBitmap ScaleBitmap(SKBitmap original, int width, int height)
    {
        var scaled = new SKBitmap(width, height);
        original.ScalePixels(scaled, SKFilterQuality.High);
        return scaled;
    }

    private void DrawText(SKCanvas canvas, string text, float x, float y, float maxWidth)
    {
        var paint = new SKPaint
        {
            Color = SKColors.Black, // Черный текст
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Center,
            TextSize = 24,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), // Жирный шрифт для лучшей читаемости
        };

        // Рисуем белый прямоугольник под текст для лучшей читаемости
        var textBounds = new SKRect();
        paint.MeasureText(text, ref textBounds);

        var backgroundRect = new SKRect(
            x - textBounds.Width / 2 - 5,
            y - textBounds.Height - 5,
            x + textBounds.Width / 2 + 5,
            y + 5
        );

        canvas.DrawRect(backgroundRect, new SKPaint { Color = SKColors.White });

        canvas.DrawText(text, x, y, paint);
    }

    private List<SKRect> GetLayout(int imageCount, int totalWidth, int totalHeight)
    {
        return imageCount switch
        {
            2 => new List<SKRect>
            {
                new SKRect(0, 0, totalWidth / 2, totalHeight),
                new SKRect(totalWidth / 2, 0, totalWidth, totalHeight),
            },
            3 => new List<SKRect>
            {
                new SKRect(0, 0, totalWidth / 2, totalHeight / 2),
                new SKRect(totalWidth / 2, 0, totalWidth, totalHeight / 2),
                new SKRect(0, totalHeight / 2, totalWidth, totalHeight),
            },
            4 => new List<SKRect>
            {
                new SKRect(0, 0, totalWidth / 2, totalHeight / 2),
                new SKRect(totalWidth / 2, 0, totalWidth, totalHeight / 2),
                new SKRect(0, totalHeight / 2, totalWidth / 2, totalHeight),
                new SKRect(totalWidth / 2, totalHeight / 2, totalWidth, totalHeight),
            },
            5 => new List<SKRect>
            {
                new SKRect(0, 0, totalWidth / 3, totalHeight / 2),
                new SKRect(totalWidth / 3, 0, 2 * totalWidth / 3, totalHeight / 2),
                new SKRect(2 * totalWidth / 3, 0, totalWidth, totalHeight / 2),
                new SKRect(0, totalHeight / 2, totalWidth / 2, totalHeight),
                new SKRect(totalWidth / 2, totalHeight / 2, totalWidth, totalHeight),
            },
            _ => throw new ArgumentException("Unsupported number of images"),
        };
    }

    private static SKBitmap AvaloniaToSkia(Bitmap avaloniaBitmap)
    {
        using var memoryStream = new MemoryStream();
        avaloniaBitmap.Save(memoryStream);
        memoryStream.Position = 0;
        return SKBitmap.Decode(memoryStream);
    }

    private static Bitmap SkiaToAvalonia(SKBitmap skiaBitmap)
    {
        using var image = SKImage.FromBitmap(skiaBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = data.AsStream();
        return new Bitmap(stream);
    }
}
