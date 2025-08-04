using System.Text;

namespace Shavkat_grabber.Models;

/// <summary>
/// Параметры (ключи) запуска приложения
/// </summary>
public class AppStartParams
{
    public bool ContainsArgs { get; private set; }

    public bool Autostart
    {
        get => field;
        set
        {
            ContainsArgs = true;
            field = value;
        }
    }

    public int AutostartGoodsCount
    {
        get => field;
        set
        {
            ContainsArgs = true;
            field = value;
        }
    }

    public bool CloseAfterPostings
    {
        get => field;
        set
        {
            ContainsArgs = true;
            field = value;
        }
    }

    public override string ToString()
    {
        if (!ContainsArgs)
            return "Без аргументов";

        StringBuilder sb = new StringBuilder();
        if (Autostart)
            sb.Append("-a " + AutostartGoodsCount);
        if (Autostart)
            sb.Append("-c");
        return sb.ToString();
    }
}
