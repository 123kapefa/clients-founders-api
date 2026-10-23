using CSharpFunctionalExtensions;

using Teledok.Domain.Enums;
using Teledok.Domain.Errors;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Entities;

public sealed class LegalEntity : Client
{
    public override ClientType Type => ClientType.LEGAL_ENTITY;

    private LegalEntity() { }

    private LegalEntity(Inn inn, ClientName name) : base(inn, name) { }

    public static Result<LegalEntity, Error> Create(Inn inn, ClientName name)
    {
        ArgumentNullException.ThrowIfNull(inn);
        ArgumentNullException.ThrowIfNull(name);

        if (!inn.IsLegalEntity)
        {
            return ClientErrors.LegalEntityInnRequired();
        }

        return new LegalEntity(inn, name);
    }
}
