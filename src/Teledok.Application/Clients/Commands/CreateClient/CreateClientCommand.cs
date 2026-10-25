using Teledok.Contracts.Clients.Requests;
using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(CreateClientRequest Request) : ICommand;
