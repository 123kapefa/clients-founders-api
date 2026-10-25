using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Founders.Commands.DeleteFounder;

public sealed record DeleteFounderCommand(Guid Id) : ICommand;
