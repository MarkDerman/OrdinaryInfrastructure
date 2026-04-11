using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.DesignContracts;
using Odin.Patterns.Commands;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection methods to support Odin command dispatching.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers the Odin implementation for ICommandDispatcher.
    /// Does not auto register all ICommandHandler of TCommand implementations.
    /// </summary>
    /// <param name="serviceCollection">The service collection to update.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOdinCommandDispatcher(this IServiceCollection serviceCollection)
    {
        Precondition.RequiresNotNull(serviceCollection);
        serviceCollection.AddOdinLoggerWrapper();
        serviceCollection.TryAddTransient<ICommandDispatcher, ServiceProviderCommandDispatcher>();
        return serviceCollection;
    }

    /// <summary>
    /// Finds all implementations of ICommandHandler (of TCommand) and
    /// ICommandHandler (of TCommand, TResult) in the application
    /// (or specific assemblies) and registers them as transient services.
    /// </summary>
    /// <param name="serviceCollection">The service collection to update.</param>
    /// <param name="assemblies">The assemblies to scan for command handler implementations.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOdinCommandHandlers(
        this IServiceCollection serviceCollection,
        params Assembly[] assemblies)
    {
        Precondition.RequiresNotNull(serviceCollection);
        Precondition.RequiresNotNull(assemblies);

        foreach (Assembly assembly in assemblies.Distinct())
        {
            Precondition.RequiresNotNull(assembly);
            RegisterCommandHandlers(serviceCollection, assembly);
        }

        return serviceCollection;
    }

    private static void RegisterCommandHandlers(IServiceCollection serviceCollection, Assembly assembly)
    {
        foreach (TypeInfo implementationType in assembly.DefinedTypes)
        {
            if (!implementationType.IsClass || implementationType.IsAbstract || implementationType.ContainsGenericParameters)
            {
                continue;
            }

            foreach (Type handlerInterfaceType in implementationType.ImplementedInterfaces.Where(IsCommandHandlerInterface))
            {
                serviceCollection.TryAddEnumerable(
                    ServiceDescriptor.Transient(handlerInterfaceType, implementationType.AsType()));
            }
        }
    }

    private static bool IsCommandHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        Type genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(ICommandHandler<>) ||
               genericTypeDefinition == typeof(ICommandHandler<,>);
    }
}
