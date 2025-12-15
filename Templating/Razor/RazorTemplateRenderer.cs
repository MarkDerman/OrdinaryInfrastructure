using System.Reflection;
using Odin.System;
using RazorLight;

namespace Odin.Templating;

/// <summary>
/// Implementation of IRazorTemplateRenderer using IRazorLightEngine
/// </summary>
public class RazorTemplateRenderer : IRazorTemplateRenderer
{
    internal readonly IRazorLightEngine _razorLightEngine;

    /// <summary>
    /// Constructor used in DI.
    /// </summary>
    /// <param name="razorLightEngine"></param>
    public RazorTemplateRenderer(IRazorLightEngine razorLightEngine)
    {
        _razorLightEngine = razorLightEngine;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="embeddedResourcesAssembly">Assembly containing</param>
    /// <param name="rootNamespace">The (period separated) case-sensitive root namespace where the template cshtml files are embedded. With or without a trailing period.</param>
    public RazorTemplateRenderer(Assembly embeddedResourcesAssembly, string rootNamespace)
    {
        _razorLightEngine = BuildRazorLightEngine(embeddedResourcesAssembly, rootNamespace);
    }

    internal static IRazorLightEngine BuildRazorLightEngine(Assembly embeddedResourcesAssembly, string? rootNamespace = null)
    {
        RazorLightOptions options = new RazorLightOptions();
        options.EnableDebugMode = true;
        return new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(embeddedResourcesAssembly, rootNamespace)
            .UseOptions(options)
            .UseMemoryCachingProvider()
            .Build();
    }

    /// <inheritdoc />
    public async Task<ResultValue<string>> RenderAsync<TModel>(string templateKey, TModel viewModel)
    {
        try
        {
            string result = await _razorLightEngine.CompileRenderAsync<TModel>(templateKey, viewModel);
            return ResultValue<string>.Succeed(result);
        }
        catch (Exception err)
        {
            return ResultValue<string>.Failure(err.Message);
        }
    }
}