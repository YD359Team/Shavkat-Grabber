using Newtonsoft.Json;

namespace Shavkat_grabber.Models.Json;

[JsonObject("usage")]
public class Usage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
    public int precached_prompt_tokens { get; set; }
}