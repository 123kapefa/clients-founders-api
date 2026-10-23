using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Teledok.Shared.Core.Abstractions;

namespace Teledok.Shared.Core;

public static class DependencyInjectionExtensions
{
    private static readonly Type[] _handlerInterfaces =
    [
        typeof(ICommandHandler<,>),
        typeof(ICommandHandler<>),
        typeof(IQueryHandler<,>),
    ];

    public static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        IEnumerable<Type> handlerTypes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false });

        foreach (Type handlerType in handlerTypes)
        {
            IEnumerable<Type> interfaces = handlerType.GetInterfaces()
                .Where(type => type.IsGenericType && _handlerInterfaces.Contains(type.GetGenericTypeDefinition()));

            foreach (Type handlerInterface in interfaces)
            {
                services.TryAddScoped(handlerType);
                services.AddScoped(handlerInterface, handlerType);
            }
        }

        return services;
    }
}
