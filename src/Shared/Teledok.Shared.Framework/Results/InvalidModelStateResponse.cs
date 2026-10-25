using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Teledok.Shared.SharedKernel;

namespace Teledok.Shared.Framework.Results;

internal static class InvalidModelStateResponse
{
    private const string CODE = "request.invalid";

    private const string MESSAGE = "Некорректный формат запроса";

    public static IActionResult Create(ActionContext context)
    {
        HashSet<string> bodyParameters = context.ActionDescriptor.Parameters
            .Where(parameter => parameter.BindingInfo?.BindingSource == BindingSource.Body)
            .Select(parameter => parameter.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        List<ErrorMessage> messages = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 } && !bodyParameters.Contains(entry.Key))
            .Select(entry => new ErrorMessage(CODE, MESSAGE, NormalizeField(entry.Key)))
            .ToList();

        Error error = messages.Count > 0
            ? Error.Validation(messages)
            : Error.Validation(CODE, MESSAGE);

        return new BadRequestObjectResult(Envelope.Fail(error));
    }

    private static string? NormalizeField(string key)
    {
        string field = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key;

        return string.IsNullOrEmpty(field) || field == "$" ? null : field;
    }

}
