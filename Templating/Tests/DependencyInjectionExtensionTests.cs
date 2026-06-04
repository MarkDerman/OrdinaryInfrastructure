using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Odin.Templating;
using RazorLight;
using System.Reflection;

namespace Tests.Odin.Templating
{
    public sealed class DependencyInjectionExtensionTests
    {
        [Test]
        public void AddRazorTemplating_succeeds()
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            Assembly testsAssembly = typeof(RazorTemplateRendererTests).Assembly;
            builder.Services.AddOdinRazorTemplating(testsAssembly, "Tests.Odin.Templating.Razor");
            WebApplication sut = builder.Build();

            IRazorTemplateRenderer? result = sut.Services.GetService<IRazorTemplateRenderer>();
            IRazorLightEngine? dependency = sut.Services.GetService<IRazorLightEngine>();

            Assert.That(result, Is.Not.Null);
            Assert.That(dependency, Is.Not.Null);
            Assert.That(result, Is.TypeOf<RazorTemplateRenderer>());
        }

    }
}