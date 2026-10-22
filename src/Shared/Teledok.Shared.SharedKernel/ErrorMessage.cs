namespace Teledok.Shared.SharedKernel;

public sealed record ErrorMessage(string Code, string Message, string? InvalidField = null);
