using System;

namespace Shavkat_grabber.Logic.Pattern;

public class Result
{
    public bool IsSuccess { get; }
    public Exception? Error { get; }

    protected Result(bool isSuccess, Exception? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Fail(Exception error) => new(false, error);

    public static Result Fail() => new(false, null);

    public static Result Success() => new(true, null);

    public static Result<T> Fail<T>(Exception error) => Result<T>.Fail(error);

    public static Result<T> Fail<T>() => Result<T>.Fail();

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool isSuccess, Exception? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success<T>(T value)
    {
        return new(value, true, null);
    }

    public static Result<T> Fail(Exception ex)
    {
        return new(default, false, ex);
    }

    public static Result<T> Fail()
    {
        return new(default, false, null);
    }
}
