using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using Teledok.Application.Clients.Commands;
using Teledok.Shared.Core;

namespace Teledok.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddHandlers(typeof(DependencyInjectionExtensions).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtensions).Assembly);
        services.AddScoped<FounderResolver>();

        return services;
    }
}
