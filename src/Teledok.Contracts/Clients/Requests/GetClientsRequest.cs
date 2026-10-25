namespace Teledok.Contracts.Clients.Requests;

public sealed record GetClientsRequest(int? Page, int? PageSize, string? Type, string? Search);
