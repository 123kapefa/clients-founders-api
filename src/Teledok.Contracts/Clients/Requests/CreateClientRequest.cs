namespace Teledok.Contracts.Clients.Requests;

public sealed record CreateClientRequest(
    string? Type,
    string? Inn,
    string? Name);
