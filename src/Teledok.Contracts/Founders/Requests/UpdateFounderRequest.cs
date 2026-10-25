namespace Teledok.Contracts.Founders.Requests;

public sealed record UpdateFounderRequest(string? LastName, string? FirstName, string? MiddleName);
