using CSharpFunctionalExtensions;

using Teledok.Domain.Enums;
using Teledok.Domain.Errors;
using Teledok.Domain.ValueObjects;
using Teledok.Shared.SharedKernel;

namespace Teledok.Domain.Entities;

public sealed class LegalEntity : Client
{
    private readonly List<LegalEntityFounder> _founders = [];

    public IReadOnlyList<LegalEntityFounder> Founders => _founders;

    private LegalEntity() { }

    private LegalEntity(Inn inn, ClientName name) : base(inn, name, ClientType.LEGAL_ENTITY) { }

    public static Result<LegalEntity, Error> Create(Inn inn, ClientName name, IEnumerable<Founder>? founders = null)
    {
        ArgumentNullException.ThrowIfNull(inn);
        ArgumentNullException.ThrowIfNull(name);

        if (!inn.IsLegalEntity)
        {
            return ClientErrors.LegalEntityInnRequired();
        }

        var legalEntity = new LegalEntity(inn, name);

        foreach (Founder founder in founders ?? [])
        {
            UnitResult<Error> added = legalEntity.AddFounder(founder);
            if (added.IsFailure)
            {
                return added.Error;
            }
        }

        return legalEntity;
    }

    public UnitResult<Error> AddFounder(Founder founder)
    {
        ArgumentNullException.ThrowIfNull(founder);

        if (_founders.Exists(link => link.FounderId == founder.Id))
        {
            return LegalEntityErrors.FounderAlreadyAdded();
        }

        _founders.Add(new LegalEntityFounder(Id, founder));

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> RemoveFounder(Guid founderId)
    {
        LegalEntityFounder? link = _founders.Find(link => link.FounderId == founderId);

        if (link is null)
        {
            return LegalEntityErrors.FounderNotFound();
        }

        _founders.Remove(link);

        return UnitResult.Success<Error>();
    }

}
