using System;
using System.Linq;
using Avalonia.Media;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Shavkat_grabber.Logic;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.ViewModels;

public class StatisticsWindowViewModel : ChildViewModel
{
    public PlotModel PlotModel { get; set; }

    public Posting[] Postings { get; set; } =
        {
            new()
            {
                Id = 1,
                CreatedAt = DateTime.Now,
                MinPrice = 100,
                MaxPrice = 250,
                ProductsCount = 1,
            },
            new()
            {
                Id = 2,
                CreatedAt = DateTime.Now + TimeSpan.FromHours(1),
                MinPrice = 400,
                MaxPrice = 600,
                ProductsCount = 3,
            },
            new()
            {
                Id = 3,
                CreatedAt = DateTime.Now + TimeSpan.FromHours(2),
                MinPrice = 100,
                MaxPrice = 200,
                ProductsCount = 3,
            },
            new()
            {
                Id = 4,
                CreatedAt = DateTime.Now + TimeSpan.FromHours(3),
                MinPrice = 75,
                MaxPrice = 140,
                ProductsCount = 4,
            },
        };

    public StatisticsWindowViewModel(
        FileSystemManager fsManager,
        WindowManager winManager,
        AppSettings settings
    )
        : base(fsManager, winManager, settings)
    {
        PlotModel = new PlotModel { Title = "Анализ товаров: цены и количество" };

        // Область для Min-Max цен
        var priceRangeSeries = new AreaSeries
        {
            Title = "Диапазон цен",
            Color = OxyColors.Blue,
            DataFieldX = "CreatedAt",
            DataFieldY = "MinPrice",
            DataFieldY2 = "MaxPrice",
            ItemsSource = Postings,
        };
        PlotModel.Series.Add(priceRangeSeries);

        // Линия для ProductsCount
        var countSeries = new LineSeries
        {
            Title = "Количество товаров",
            Color = OxyColors.Green,
            StrokeThickness = 2,
            DataFieldX = "CreatedAt",
            DataFieldY = "ProductsCount",
            ItemsSource = Postings,
            YAxisKey = "CountAxis", // Привязка к правой оси
        };
        PlotModel.Series.Add(countSeries);

        // Настройка осей (аналогично первому примеру)
        PlotModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, Title = "Дата" });
        PlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Цена" });
        PlotModel.Axes.Add(
            new LinearAxis
            {
                Position = AxisPosition.Right,
                Title = "Количество",
                Key = "CountAxis",
            }
        );
    }
}
