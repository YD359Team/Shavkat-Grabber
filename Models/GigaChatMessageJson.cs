using System.Collections.Generic;
using Newtonsoft.Json;

namespace Shavkat_grabber.Models;

public class GigaChatMessageJson
{
    public string model { get; set; }
    public List<GigaChatMessage> messages { get; set; }
}

public class GigaChatMessage
{
    public string content { get; set; }
    public string role { get; set; }
}

public class OAuth2Token
{
    public string access_token { get; set; }

    public long expires_at { get; set; }
}

[JsonObject("choice")]
public class Choice
{
    public Message message { get; set; }
    public int index { get; set; }
    public string finish_reason { get; set; }
}

[JsonObject("message")]
public class Message
{
    public string content { get; set; }
    public string role { get; set; }
}

[JsonObject("root")]
public class AnswerRoot
{
    public List<Choice> choices { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public string @object { get; set; }
    public Usage usage { get; set; }
}

[JsonObject("usage")]
public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
    public int precached_prompt_tokens { get; set; }
}
