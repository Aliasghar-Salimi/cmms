using System.Collections.Generic;

namespace IdentityService.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public T? Data => Value; // Alias for Value to match controller expectations
    public string? Error { get; }
    public List<string> ValidationErrors { get; } = new();
    public string? ValidationFailure => ValidationErrors.Count > 0 ? string.Join(", ", ValidationErrors) : null;

    private Result(bool isSuccess, T? value, string? error, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        if (validationErrors != null)
        {
            ValidationErrors.AddRange(validationErrors);
        }
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public List<string> ValidationErrors { get; } = new();
    public string? ValidationFailure => ValidationErrors.Count > 0 ? string.Join(", ", ValidationErrors) : null;

    private Result(bool isSuccess, string? error, List<string>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        if (validationErrors != null)
        {
            ValidationErrors.AddRange(validationErrors);
        }
    }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
} 