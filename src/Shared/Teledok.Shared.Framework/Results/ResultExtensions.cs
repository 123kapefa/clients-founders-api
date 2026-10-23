using CSharpFunctionalExtensions;

using Microsoft.AspNetCore.Mvc;

using Teledok.Shared.SharedKernel;

namespace Teledok.Shared.Framework.Results;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<TValue>(this Result<TValue, Error> result) =>
        result.IsSuccess
            ? new OkObjectResult(Envelope<TValue>.Ok(result.Value))
            : ToErrorResult(result.Error);

    public static IActionResult ToCreatedResult<TValue>(this Result<TValue, Error> result, Func<TValue, string> location)
    {
        ArgumentNullException.ThrowIfNull(location);

        return result.IsSuccess
            ? new CreatedResult(location(result.Value), Envelope<TValue>.Ok(result.Value))
            : ToErrorResult(result.Error);
    }

    public static IActionResult ToActionResult(this UnitResult<Error> result) =>
        result.IsSuccess
            ? new OkObjectResult(Envelope.Ok())
            : ToErrorResult(result.Error);

    private static ObjectResult ToErrorResult(Error error) =>
        new(Envelope.Fail(error)) { StatusCode = error.Type.ToStatusCode() };
}
