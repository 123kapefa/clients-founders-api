using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Clients.Commands.RemoveFounder;

public sealed record RemoveFounderCommand(Guid ClientId, Guid FounderId) : ICommand;
