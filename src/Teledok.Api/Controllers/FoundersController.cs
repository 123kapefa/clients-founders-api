using Microsoft.AspNetCore.Mvc;

using Teledok.Application.Founders.Commands.DeleteFounder;
using Teledok.Application.Founders.Commands.UpdateFounder;
using Teledok.Application.Founders.Queries.GetFounderById;
using Teledok.Application.Founders.Queries.GetFounders;
using Teledok.Contracts.Founders.Requests;
using Teledok.Contracts.Founders.Responses;
using Teledok.Shared.Framework.Results;
using Teledok.Shared.SharedKernel;

namespace Teledok.Api.Controllers;

[ApiController]
[Route("api/founders")]
[Produces("application/json")]
public sealed class FoundersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<Envelope<PageResult<FounderResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList(
        [FromQuery] GetFoundersRequest request,
        [FromServices] GetFoundersHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetFoundersQuery(request), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Envelope<FounderDetailsResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        [FromServices] GetFounderByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetFounderByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<Envelope<FounderResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateFounderRequest request,
        [FromServices] UpdateFounderHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new UpdateFounderCommand(id, request), cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<Envelope>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Envelope>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] DeleteFounderHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeleteFounderCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}
