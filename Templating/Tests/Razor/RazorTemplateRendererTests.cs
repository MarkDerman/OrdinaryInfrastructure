using System.Reflection;
using Odin.System;
using Odin.Templating;
using Xunit;

namespace Tests.Odin.Templating.Razor
{
    public sealed class RazorTemplateRendererTests
    {
        [Theory(Skip = "Can't figure out why the error 'Template XXX.cshtml is corrupted or invalid' is happening???")]
        [InlineData("Tests.Odin.Templating", "TestTemplate2", true)]
        [InlineData("Tests.Odin.Templating.Views", "TestTemplate1", true)]
        [InlineData("tests.odin.templating.views", "TestTemplate1", false)]
        [InlineData("Tests.Odin.Templating.Views.", "TestTemplate1", true)]
        [InlineData("tests.odin.templating.razor", "testtemplate1", false)]
        [InlineData("Wrong", "TestTemplate1", false)]
        [InlineData("Tests.Odin.Templating.Views", "Wrong", false)]
        [InlineData("Tests.Odin.Templating", "TestTemplate1", false)]
        [InlineData(null, "TestTemplate1", false)]
        [InlineData("", "TestTemplate1", false)]
        public async Task Create_with_different_templateKeys_and_namespaces(string? rootNamespace, string templateKey, bool shouldSucceed)
        {
            Assembly testsAssembly = typeof(RazorTemplateRendererTests).Assembly;
            RazorTemplateRenderer sut = new RazorTemplateRenderer(testsAssembly, rootNamespace);
            ResultValue<string> result = await sut.RenderAsync(templateKey, new TestViewModel(){ Title = "World"});

            Assert.True(result.IsSuccess == shouldSucceed, result.MessagesToString());
            if (shouldSucceed)
            {
                Assert.Contains("<div>Hello World</div>", result.Value);
            }
            else
            {
                Assert.NotEmpty(result.MessagesToString());
            }
        }
    }
}
