namespace Teledok.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; }

    protected Entity()
    {
        Id = Guid.CreateVersion7();
    }
}
