using CSharpFunctionalExtensions;

using Teledok.Domain.Common;
using Teledok.Domain.Enums;
using Teledok.Domain.Errors;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Entities;

public sealed class IndividualEntrepreneur : Client
{
    public override ClientType Type => ClientType.INDIVIDUAL_ENTREPRENEUR;

    private IndividualEntrepreneur() { }

    private IndividualEntrepreneur(Inn inn, ClientName name) : base(inn, name) { }

    public static Result<IndividualEntrepreneur, Error> Create(Inn inn, ClientName name)
    {
        ArgumentNullException.ThrowIfNull(inn);
        ArgumentNullException.ThrowIfNull(name);

        if (!inn.IsPerson)
        {
            return ClientErrors.IndividualEntrepreneurInnRequired();
        }

        return new IndividualEntrepreneur(inn, name);
    }
}
