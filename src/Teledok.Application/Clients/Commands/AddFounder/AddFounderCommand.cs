using Teledok.Contracts.Clients.Requests;
using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Clients.Commands.AddFounder;

public sealed record AddFounderCommand(Guid ClientId, AddFounderRequest Request) : ICommand;
