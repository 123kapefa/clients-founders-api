namespace Teledok.Contracts.Clients.Requests;

public sealed record AddFounderRequest(
    Guid? FounderId,
    string? Inn,
    string? LastName,
    string? FirstName,
    string? MiddleName);
