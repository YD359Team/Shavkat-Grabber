using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Shavkat_grabber.Models;
using SwiftExcel;

namespace Shavkat_grabber.Logic;

public class Exporter
{
    public async Task ToCsv(IEnumerable<Good> goods, string filePath)
    {
        await using StreamWriter sw = new StreamWriter(filePath);
        foreach (var item in goods)
        {
            string[] a = [item.Article, item.Title, item.Price, item.Url, item.ImageUrl];
            string ch = string.Join(";;", a);
            await sw.WriteLineAsync(ch);
        }
        sw.Close();
    }

    public async Task ToXlsx(IEnumerable<Good> goods, string filePath)
    {
        int row = 1;
        using var writer = new SwiftExcel.ExcelWriter(filePath);
        foreach (var item in goods)
        {
            writer.Write(item.Article, 1, row);
            writer.Write(item.Title, 2, row);
            writer.Write(item.Price, 3, row);
            writer.Write(item.Url, 6, row);
            writer.Write(item.ImageUrl, 7, row);
            row++;
        }
    }
}
