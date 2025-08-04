using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Shavkat_grabber.Logic.Pattern;
using Shavkat_grabber.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Message = Telegram.Bot.Types.Message;

namespace Shavkat_grabber.Logic.API;

public class TelegramBotApi
{
    private readonly TelegramBotClient _client;
    private readonly string _chatId;

    public TelegramBotApi(string token, string channelId)
    {
        _chatId = channelId;
        _client = new TelegramBotClient(token);
    }

    public async Task<Result<int>> Post(string postText, Bitmap[] attachments)
    {
        // Создаем список потоков для изображений
        var streams = new MemoryStream[attachments.Length];
        var media = attachments
            .Select(
                (bitmap, index) =>
                {
                    // Преобразуем Bitmap в поток
                    streams[index] = new MemoryStream();
                    bitmap.Save(streams[index]); // Сохраняем Bitmap в поток
                    streams[index].Position = 0; // Сбрасываем позицию потока

                    // Создаем InputMediaPhoto
                    var mediaPhoto = new InputMediaPhoto(
                        new InputFileStream(streams[index], $"image_{index}.png")
                    );
                    if (index == 0) // Устанавливаем подпись для первого изображения
                    {
                        mediaPhoto.ParseMode = ParseMode.MarkdownV2;
                        mediaPhoto.Caption = EscapeMarkdownV2(postText);
                    }

                    return mediaPhoto;
                }
            )
            .ToArray();

        try
        {
            // Отправляем группу медиа в Telegram-канал
            Message[] messages = await _client.SendMediaGroup(_chatId, media);
            return Result.Success(messages.First().Id);
        }
        catch (Exception ex)
        {
            return Result.Fail<int>(ex);
        }
        finally
        {
            // Закрываем все потоки после отправки
            foreach (var stream in streams)
            {
                stream?.Dispose();
            }
        }
    }

    public async Task<Result> CheckConnection()
    {
        try
        {
            User me = await _client.GetMe();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex);
        }
    }

    private static string EscapeMarkdownV2(string text)
    {
        char[] specialChars =
        { /*'_', '*',
            '[',
            ']',
            '(',
            ')',*/
            '~',
            '`',
            '>',
            '#',
            '+',
            '-',
            '=',
            '|',
            '{',
            '}',
            '.',
            '!',
        };
        foreach (var c in specialChars)
        {
            text = text.Replace(c.ToString(), $"\\{c}");
        }
        return text;
    }
}
