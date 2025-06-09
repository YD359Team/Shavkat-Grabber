using System;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Helpers;

public static class DebugHelper
{
    private static int _counter = 0;

    public static Good GetTestGood()
    {
        _counter++;
        int oldPrice = Random.Shared.Next(100, 10000);
        int price = (int)(oldPrice * 0.75f);
        return new Good()
        {
            Article = Guid.NewGuid().ToString(),
            Url = $"https://www.wildberries.ru/catalog/{_counter}/detail.aspx",
            ImageUrl = $"https://api.slingacademy.com/public/sample-photos/{_counter}.jpeg",
            Price = price.ToString(),
            Title = $"Тестовый товар #{_counter}",
        };
    }
}
