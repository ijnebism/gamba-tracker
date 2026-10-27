namespace GambaTracker.Api.Services;

// Outcome of a service write, so controllers can map it to an HTTP status without knowing why it failed
public enum OperationStatus { Success, NotFound, NameTaken }

public record OperationResult<T>(OperationStatus Status, T? Value = default)
{
    public static OperationResult<T> Success(T value) => new(OperationStatus.Success, value);
    public static OperationResult<T> NotFound() => new(OperationStatus.NotFound);
    public static OperationResult<T> NameTaken() => new(OperationStatus.NameTaken);
}
