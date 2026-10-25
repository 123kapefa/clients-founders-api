using Microsoft.AspNetCore.Mvc;

using Teledok.Application.Clients.Commands.CreateClient;
using Teledok.Application.Clients.Commands.DeleteClient;
using Teledok.Application.Clients.Commands.UpdateClient;
using Teledok.Application.Clients.Queries.GetClientById;
using Teledok.Application.Clients.Queries.GetClients;
using Teledok.Contracts.Clients.Requests;
using Teledok.Contracts.Clients.Responses;
using Teledok.Shared.Framework.Results;
using Teledok.Shared.SharedKernel;

namespace Teledok.Api.Controllers;

[ApiController]
[Route("api/clients")]
[Produces("application/json")]
public sealed class ClientsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(StatusCodes.Status201Created)]
    [ProducesResponseType<Envelope>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
    [FromBody] CreateClientRequest request,
    [FromServices] CreateClientHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new CreateClientCommand(request), cancellationToken);

        return result.ToCreatedResult(id => $"/api/clients/{id}");
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<Envelope<ClientResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
    Guid id,
    [FromServices] GetClientByIdHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetClientByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet]
    [ProducesResponseType<Envelope<PageResult<ClientResponse>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList(
    [FromQuery] GetClientsRequest request,
    [FromServices] GetClientsHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetClientsQuery(request), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<Envelope<ClientResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Envelope>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
    Guid id,
    [FromBody] UpdateClientRequest request,
    [FromServices] UpdateClientHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new UpdateClientCommand(id, request), cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType<Envelope>(StatusCodes.Status200OK)]
    [ProducesResponseType<Envelope>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
    Guid id,
    [FromServices] DeleteClientHandler handler,
    CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new DeleteClientCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}
