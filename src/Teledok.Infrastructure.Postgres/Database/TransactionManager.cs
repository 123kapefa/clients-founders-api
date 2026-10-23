using CSharpFunctionalExtensions;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Teledok.Shared.Core.Database;
using Teledok.Shared.SharedKernel;

namespace Teledok.Infrastructure.Postgres.Database;

internal sealed class TransactionManager(TeledokDbContext dbContext) : ITransactionManager
{
    private static readonly Dictionary<string, string> _uniqueConstraintFields = new(StringComparer.Ordinal)
    {
        ["ix_clients_inn"] = "inn",
        ["ix_founders_inn"] = "inn",
        ["ix_legal_entity_founders_legal_entity_id_founder_id"] = "founderId",
    };

    public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (DbUpdateConcurrencyException)
        {
            return GeneralErrors.ConcurrencyConflict();
        }
        catch (DbUpdateException exception) when (IsPostgresError(exception, PostgresErrorCodes.UniqueViolation))
        {
            string? constraintName = ((PostgresException)exception.InnerException!).ConstraintName;
            string? fieldName = constraintName is null ? null : _uniqueConstraintFields.GetValueOrDefault(constraintName);

            return GeneralErrors.UniqueConstraintViolation(fieldName);
        }
        catch (DbUpdateException exception) when (IsPostgresError(exception, PostgresErrorCodes.ForeignKeyViolation))
        {
            return GeneralErrors.ForeignKeyViolation();
        }
    }

    private static bool IsPostgresError(DbUpdateException exception, string sqlState) =>
        exception.InnerException is PostgresException postgresException
        && string.Equals(postgresException.SqlState, sqlState, StringComparison.Ordinal);
}
