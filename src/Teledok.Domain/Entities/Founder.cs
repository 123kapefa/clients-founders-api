using CSharpFunctionalExtensions;

using Teledok.Domain.Common;
using Teledok.Domain.Errors;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Entities;

public sealed class Founder : Entity, IAuditable, ISoftDeletable
{
    public Inn Inn { get; private set; } = null!;

    public FullName FullName { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    private Founder() { }

    private Founder(Inn inn, FullName fullName)
    {
        Inn = inn;
        FullName = fullName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsDeleted = false;
        DeletedAt = null;
    }

    public static Result<Founder, Error> Create(Inn inn, FullName fullName)
    {
        ArgumentNullException.ThrowIfNull(inn);
        ArgumentNullException.ThrowIfNull(fullName);

        if (!inn.IsPerson)
        {
            return FounderErrors.InnMustBelongToPerson();
        }

        return new Founder(inn, fullName);
    }

    public void Rename(FullName fullName)
    {
        ArgumentNullException.ThrowIfNull(fullName);

        FullName = fullName;
    }
}
