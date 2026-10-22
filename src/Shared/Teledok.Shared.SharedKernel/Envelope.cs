namespace Teledok.Shared.SharedKernel;

public sealed record Envelope
{
    public object? Result { get; }

    public Error? Error { get; }

    public bool IsError => Error is not null;

    public DateTime TimeGenerated { get; }

    private Envelope(Error? error)
    {
        Result = null;
        Error = error;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope Ok() => new(error: null);

    public static Envelope Fail(Error error) => new(error);
}

public sealed record Envelope<T>
{
    public T? Result { get; }

    public Error? Error { get; }

    public bool IsError => Error is not null;

    public DateTime TimeGenerated { get; }

    private Envelope(T? result, Error? error)
    {
        Result = result;
        Error = error;
        TimeGenerated = DateTime.UtcNow;
    }

    public static Envelope<T> Ok(T? result = default) => new(result, null);

    public static Envelope<T> Fail(Error error) => new(default, error);
}
