using System;

namespace Shavkat_grabber.Models;

public class ChatMessage
{
    public ChatMessage(int id, string text, ChatMessageTypes type)
    {
        Id = id;
        Text = text;
        Type = type;
        Time = DateTime.Now.ToString("mm:ss");
    }

    public int Id { get; set; }
    public string Text { get; set; }
    public ChatMessageTypes Type { get; set; }
    public string Time { get; set; }
}

public enum ChatMessageTypes
{
    Quest,
    Answer,
    Error,
}
