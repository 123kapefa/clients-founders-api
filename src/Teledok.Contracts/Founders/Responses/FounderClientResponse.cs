namespace Teledok.Contracts.Founders.Responses;

public sealed record FounderClientResponse(
    Guid ClientId,
    string Inn,
    string Name,
    DateTime AddedAt);
