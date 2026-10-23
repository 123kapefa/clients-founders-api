using Teledok.Domain.Common;
using Teledok.Domain.Enums;
using Teledok.Domain.ValueObjects;

namespace Teledok.Domain.Entities;

public abstract class Client : Entity, IAuditable, ISoftDeletable
{
    public Inn Inn { get; private set; } = null!;

    public ClientName Name { get; private set; } = null!;

    public abstract ClientType Type { get; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    protected Client() { }

    protected Client(Inn inn, ClientName name)
    {
        Inn = inn;
        Name = name;
    }

    public void Rename(ClientName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
    }
}
