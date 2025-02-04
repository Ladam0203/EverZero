namespace AuthService.Core;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public IEnumerable<string>? Errors { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Errors = null;
    }

    private Result(IEnumerable<string> errors)
    {
        IsSuccess = false;
        Errors = errors;
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(IEnumerable<string> errors) => new(errors);
}
