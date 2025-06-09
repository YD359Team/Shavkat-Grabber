using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shavkat_grabber.Models;
using Shavkat_grabber.Models.Json;
using Shavkat_grabber.Pattern;

namespace Shavkat_grabber.Logic;

public class TextController
{
    public async Task<Result<TextVariants>> CreateTextVariantsAsync(
        AppSettings appSettings,
        Good[] goods,
        GigaChatApi gigaChatApi,
        bool useGigaChat
    )
    {
        try
        {
            bool isSingleGood = goods.Length == 1;

            StringBuilder sbText = new();
            StringBuilder sbMd = new();

            if (!string.IsNullOrWhiteSpace(appSettings.StaticHeader))
            {
                sbText.AppendLine(appSettings.StaticHeader);
                sbText.AppendLine();
                sbMd.AppendLine(appSettings.StaticHeader);
                sbMd.AppendLine();
            }

            if (useGigaChat)
            {
                string quest =
                    appSettings.GigaChatPrompt + " " + string.Join(';', goods.Select(x => x.Title));
                Result<AnswerRoot> answer = await gigaChatApi.SendMessage(quest);

                if (answer.IsSuccess)
                {
                    string result = answer
                        .Value.choices.First()
                        .message.content.Replace(@"\n", "\n")
                        .Replace("**", "*")
                        .Replace("__", "_")
                        .Replace("(", @"\(")
                        .Replace(")", @"\)");
                    sbText.AppendLine(result);
                    sbText.AppendLine();
                    sbMd.AppendLine(result);
                    sbMd.AppendLine();
                }
            }

            for (var index = 0; index < goods.Length; index++)
            {
                var good = goods[index];

                if (isSingleGood)
                {
                    sbText.AppendLine($"{good.Title} {good.Url}");
                    sbMd.AppendLine($"[{good.Title}]({good.Url})");
                }
                else
                {
                    sbText.AppendLine($"{index + 1}. {good.Title} {good.Url}");
                    sbMd.AppendLine($"{index + 1}. [{good.Title}]({good.Url})");
                }

                sbText.AppendLine($"💰 {good.Price}");
                sbMd.AppendLine($"💰 {good.Price}");
            }

            if (!string.IsNullOrWhiteSpace(appSettings.StaticFooter))
            {
                sbText.AppendLine();
                sbText.AppendLine(appSettings.StaticFooter);
                sbMd.AppendLine();
                sbMd.AppendLine(appSettings.StaticFooter);
            }

            return Result.Success(new TextVariants(sbText.ToString(), sbMd.ToString()));
        }
        catch (Exception ex)
        {
            return Result<TextVariants>.Fail(ex);
        }
    }
}
