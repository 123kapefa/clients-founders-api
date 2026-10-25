namespace Teledok.Shared.SharedKernel;

public sealed record Error
{
    public IReadOnlyList<ErrorMessage> Messages { get; }

    public ErrorType Type { get; }

    public ErrorBehavior Behavior { get; }

    private Error(IReadOnlyList<ErrorMessage> messages, ErrorType type, ErrorBehavior behavior = ErrorBehavior.PERMANENT)
    {
        if (messages.Count == 0)
        {
            throw new ArgumentException("Error must contain at least one message.", nameof(messages));
        }

        Messages = [.. messages];
        Type = type;
        Behavior = behavior;
    }

    public string GetMessage() => string.Join(';', Messages.Select(message => $"{message.Code}: {message.Message}"));

    public static Error Validation(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.VALIDATION);

    public static Error Validation(IEnumerable<ErrorMessage> messages) =>
        new([.. messages], ErrorType.VALIDATION);

    public static Error NotFound(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.NOT_FOUND);

    public static Error Failure(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.FAILURE);

    public static Error Conflict(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.CONFLICT);

    public static Error Authentication(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.AUTHENTICATION);

    public static Error Authorization(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.AUTHORIZATION);

    public static Error RateLimit(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.RATE_LIMIT);

    public Error AsTransient() => new(Messages, Type, ErrorBehavior.TRANSIENT);

    public Error AsPermanent() => new(Messages, Type, ErrorBehavior.PERMANENT);

    public Error WithFieldPrefix(string prefix) => new([.. Messages.Select(message => message with {
            InvalidField = message.InvalidField is null ? prefix : $"{prefix}.{message.InvalidField}",
        })], Type, Behavior);

    public bool Equals(Error? other) =>
        other is not null
        && Type == other.Type
        && Behavior == other.Behavior
        && Messages.SequenceEqual(other.Messages);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Type);
        hash.Add(Behavior);

        foreach (ErrorMessage message in Messages)
        {
            hash.Add(message);
        }

        return hash.ToHashCode();
    }
}
