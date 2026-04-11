using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.DesignContracts;
using Odin.Patterns.Queries;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection methods to support Odin query dispatching.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers the Odin implementation for <see cref="IQueryDispatcher"/>.
    /// Does not auto register all <see cref="IQueryHandler{TQuery,TQueryResult}"/> implementations.
    /// </summary>
    /// <param name="serviceCollection">The service collection to update.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOdinQueryDispatcher(this IServiceCollection serviceCollection)
    {
        Precondition.RequiresNotNull(serviceCollection);
        serviceCollection.AddOdinLoggerWrapper();
        serviceCollection.TryAddTransient<IQueryDispatcher, ServiceProviderQueryDispatcher>();
        return serviceCollection;
    }

    /// <summary>
    /// Finds all implementations of <see cref="IQueryHandler{TQuery,TQueryResult}"/> in the specified assemblies,
    /// and registers them as transient services.
    /// </summary>
    /// <param name="serviceCollection">The service collection to update.</param>
    /// <param name="assemblies">The assemblies to scan for query handler implementations.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOdinQueryHandlers(
        this IServiceCollection serviceCollection,
        params Assembly[] assemblies)
    {
        Precondition.RequiresNotNull(serviceCollection);
        Precondition.RequiresNotNull(assemblies);

        foreach (Assembly assembly in assemblies.Distinct())
        {
            Precondition.RequiresNotNull(assembly);
            RegisterQueryHandlers(serviceCollection, assembly);
        }

        return serviceCollection;
    }

    private static void RegisterQueryHandlers(IServiceCollection serviceCollection, Assembly assembly)
    {
        foreach (TypeInfo implementationType in assembly.DefinedTypes)
        {
            if (!implementationType.IsClass || implementationType.IsAbstract || implementationType.ContainsGenericParameters)
            {
                continue;
            }

            foreach (Type handlerInterfaceType in implementationType.ImplementedInterfaces.Where(IsQueryHandlerInterface))
            {
                serviceCollection.TryAddEnumerable(
                    ServiceDescriptor.Transient(handlerInterfaceType, implementationType.AsType()));
            }
        }
    }

    private static bool IsQueryHandlerInterface(Type type)
    {
        if (!type.IsGenericType)
        {
            return false;
        }

        return type.GetGenericTypeDefinition() == typeof(IQueryHandler<,>);
    }
}
