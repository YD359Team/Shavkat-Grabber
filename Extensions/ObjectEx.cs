using Shavkat_grabber.Pattern;

namespace Shavkat_grabber.Extensions;

public static class ObjectEx
{
    public static Result<T> ToResult<T>(this T value)
    {
        return Result.Success(value);
    }
}
