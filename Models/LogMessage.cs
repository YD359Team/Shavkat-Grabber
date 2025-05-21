using System;

namespace Shavkat_grabber.Models;

/// <summary>
/// Сообщения для виртуального лога (консоли) парсинга
/// </summary>
public class LogMessage
{
    public string Message { get; set; }
    public LogMessageTypes MessageType { get; set; }

    private TimeOnly _createTime;
    public string CreateTime => $"{_createTime:HH:mm:ss}";

    public LogMessage(string msg, LogMessageTypes msgType)
    {
        Message = msg;
        MessageType = msgType;
        DateTime now = DateTime.Now;
        _createTime = new TimeOnly(now.Hour, now.Minute, now.Second);
    }
}

public enum LogMessageTypes
{
    Trace,
    Success,
    Warning,
    Error,
}
