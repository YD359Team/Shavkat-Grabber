using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Shavkat_grabber.Logic.Pattern;
using Shavkat_grabber.Models;
using Shavkat_grabber.Models.Json;
using YDs_DLL_BASE.Extensions;

namespace Shavkat_grabber.Logic;

public class TextController
{
    public string AddMarkdownLinks(string postText, Product[] goods)
    {
        char botMarker = '—';

        Regex regex = new Regex($@"(.+)\s*{botMarker}\s*");
        int currentIndex = 0;
        return regex.Replace(
            postText,
            (m) =>
            {
                return $"[{m.Groups[1].Value}]({goods[currentIndex++].Url}) - ";
            }
        );
    }

    public string AddLinkToTelegram(string postText, AppSettings settings, int postId)
    {
        return string.Concat(postText, $"\nhttps://t.me/{settings.TgChannelId[1..]}/{postId}");
    }
}
