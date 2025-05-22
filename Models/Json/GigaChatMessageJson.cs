using System.Collections.Generic;

namespace Shavkat_grabber.Models.Json;

public class GigaChatMessageJson
{
    public string model { get; set; }
    public List<GigaChatMessage> messages { get; set; }
}
