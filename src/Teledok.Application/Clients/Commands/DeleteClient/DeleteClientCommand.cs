using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Clients.Commands.DeleteClient;

public sealed record DeleteClientCommand(Guid Id) : ICommand;
