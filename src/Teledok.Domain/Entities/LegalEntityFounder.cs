using Teledok.Domain.Common;

namespace Teledok.Domain.Entities;

public sealed class LegalEntityFounder : Entity, IAuditable, ISoftDeletable
{
    public Guid LegalEntityId { get; private set; }

    public Guid FounderId { get; private set; }

    public Founder Founder { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    private LegalEntityFounder() { }

    internal LegalEntityFounder(Guid legalEntityId, Founder founder)
    {
        ArgumentNullException.ThrowIfNull(founder);

        LegalEntityId = legalEntityId;
        FounderId = founder.Id;
        Founder = founder;
    }
}
