using Teledok.Contracts.Founders.Requests;
using Teledok.Shared.Core.Abstractions;

namespace Teledok.Application.Founders.Commands.UpdateFounder;

public sealed record UpdateFounderCommand(Guid Id, UpdateFounderRequest Request) : ICommand;
