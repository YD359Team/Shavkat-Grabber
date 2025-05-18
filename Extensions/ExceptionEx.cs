using System;
using System.Linq;

namespace Shavkat_grabber.Extensions;

public static class ExceptionEx
{
    public static string GetMessage(this Exception ex)
    {
        if (ex is AggregateException aex)
        {
            return string.Join("\n", aex.InnerExceptions.Select((x, i) => $"{i}) + {x.Message}"));
        }

        return ex.Message;
    }
}
