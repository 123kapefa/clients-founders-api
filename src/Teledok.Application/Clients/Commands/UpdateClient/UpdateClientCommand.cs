using Teledok.Contracts.Clients.Requests;
using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(Guid Id, UpdateClientRequest Request) : ICommand;
