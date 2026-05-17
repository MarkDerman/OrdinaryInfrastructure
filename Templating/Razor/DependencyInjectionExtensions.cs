using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.Templating;
using RazorLight;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Methods for adding these services to the application.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds an IRazorTemplateRenderer into dependency injection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="embeddedResourcesAssembly">The assembly containing the .cshtml Razor template files as embedded resources.</param>
    /// <param name="rootNamespace">The (period separated) root namespace where the template cshtml files are embedded. With or without a trailing period.</param>
    /// <returns></returns>
    public static IServiceCollection AddOdinRazorTemplating(this IServiceCollection services, Assembly embeddedResourcesAssembly, string rootNamespace)
    {
        services.AddOdinLoggerWrapper();
        services.AddMemoryCache();
        // Take advantage of the compiled templates cache in RazorLight. Add it as a singleton.
        services.TryAddSingleton<IRazorLightEngine>(_ => RazorTemplateRenderer.BuildRazorLightEngine(embeddedResourcesAssembly, rootNamespace));
        services.TryAddTransient<IRazorTemplateRenderer, RazorTemplateRenderer>();
        return services;
    }


}