using Newtonsoft.Json;

namespace Shavkat_grabber.Models.Json;

[JsonObject("message")]
public class Message
{
    public string content { get; set; }
    public string role { get; set; }
}