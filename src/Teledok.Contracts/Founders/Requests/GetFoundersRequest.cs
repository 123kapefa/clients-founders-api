namespace Teledok.Contracts.Founders.Requests;

public sealed record GetFoundersRequest(int? Page, int? PageSize, string? Search);
