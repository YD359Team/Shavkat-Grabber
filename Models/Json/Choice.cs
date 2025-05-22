using Newtonsoft.Json;

namespace Shavkat_grabber.Models.Json;

[JsonObject("choice")]
public class Choice
{
    public Message message { get; set; }
    public int index { get; set; }
    public string finish_reason { get; set; }
}