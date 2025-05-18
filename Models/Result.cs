using System;

namespace Shavkat_grabber.Models;

public class Result
{
    public bool IsSuccess { get; set; }
    public Exception? Error { get; set; }
}

public class Result<T> : Result
{
    public T? Value { get; set; }

    private Result(T? value, bool isSuccess, Exception? error)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<T> Success<T>(T value)
    {
        return new(value, true, null);
    }

    public static Result<T> Fail(Exception ex)
    {
        return new(default, false, ex);
    }
}
