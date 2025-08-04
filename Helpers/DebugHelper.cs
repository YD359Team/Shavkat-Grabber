using System;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Helpers;

public static class DebugHelper
{
    private static int _counter = 0;

    public static Product GetTestGood()
    {
        string[] names =
        [
            "Пароварка универсальная",
            "Утюжок-отпариватель для одежды",
            "Хлеборезка компактная",
            "Нож разделочный кухонный",
            "Точилка для карандаша школьная",
            "Перчатки мужские черные",
            "Мышка Logitech",
            "Клавиатура Logitech",
        ];

        _counter++;
        int price = Random.Shared.Next(100, 10000);
        return new Product()
        {
            Article = Guid.NewGuid().ToString(),
            Url = $"https://www.wildberries.ru/catalog/{_counter}/detail.aspx",
            ImageUrl = $"https://api.slingacademy.com/public/sample-photos/{_counter}.jpeg",
            Price = price + ".00",
            Title = $"{names[Random.Shared.Next(names.Length)]} #{_counter}",
        };
    }
}
