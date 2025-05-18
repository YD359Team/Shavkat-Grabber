using Newtonsoft.Json;

namespace Shavkat_grabber.Helpers;

public static class SerializeHelper
{
    public static string Serialize(object obj)
    {
        return JsonConvert.SerializeObject(obj, Formatting.Indented);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}