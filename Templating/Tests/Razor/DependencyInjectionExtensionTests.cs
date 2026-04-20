using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Odin.Templating;
using RazorLight;
using Xunit;

namespace Tests.Odin.Templating.Razor
{
    public sealed class DependencyInjectionExtensionTests
    {
        [Fact]
        public void AddRazorTemplating_succeeds()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            Assembly testsAssembly = typeof(RazorTemplateRendererTests).Assembly;
            builder.Services.AddOdinRazorTemplating(testsAssembly, "Tests.Odin.Templating.Razor");
            WebApplication sut = builder.Build();
            
            IRazorTemplateRenderer? result = sut.Services.GetService<IRazorTemplateRenderer>();
            IRazorLightEngine? dependency = sut.Services.GetService<IRazorLightEngine>();
            
            Assert.NotNull(result);
            Assert.NotNull(dependency);
            Assert.IsType<RazorTemplateRenderer>(result);
        }
        
    }
}
