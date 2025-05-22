using System.Collections.Generic;
using Newtonsoft.Json;

namespace Shavkat_grabber.Models.Json;

[JsonObject("root")]
public class AnswerRoot
{
    public List<Choice> choices { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public string @object { get; set; }
    public Usage usage { get; set; }
}