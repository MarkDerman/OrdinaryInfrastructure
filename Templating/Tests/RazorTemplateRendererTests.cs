using Odin.System;
using Odin.Templating;
using System.Reflection;

namespace Tests.Odin.Templating
{
    public sealed class RazorTemplateRendererTests
    {
        [Ignore("Can't figure out why the error 'Template XXX.cshtml is corrupted or invalid' is happening???")]
        [TestCase("Tests.Odin.Templating", "TestTemplate2", true)]
        [TestCase("Tests.Odin.Templating.Views", "TestTemplate1", true)]
        [TestCase("tests.odin.templating.views", "TestTemplate1", false)]
        [TestCase("Tests.Odin.Templating.Views.", "TestTemplate1", true)]
        [TestCase("tests.odin.templating.razor", "testtemplate1", false)]
        [TestCase("Wrong", "TestTemplate1", false)]
        [TestCase("Tests.Odin.Templating.Views", "Wrong", false)]
        [TestCase("Tests.Odin.Templating", "TestTemplate1", false)]
        [TestCase(null, "TestTemplate1", false)]
        [TestCase("", "TestTemplate1", false)]
        public async Task Create_with_different_templateKeys_and_namespaces(string? rootNamespace, string templateKey, bool shouldSucceed)
        {
            Assembly testsAssembly = typeof(RazorTemplateRendererTests).Assembly;
            RazorTemplateRenderer sut = new RazorTemplateRenderer(testsAssembly, rootNamespace);
            ResultValue<string> result = await sut.RenderAsync(templateKey, new TestViewModel() { Title = "World" });

            Assert.That(result.IsSuccess == shouldSucceed, Is.True, result.MessagesToString());
            if (shouldSucceed)
            {
                Assert.That(result.Value, Does.Contain("<div>Hello World</div>"));
            }
            else
            {
                Assert.That(result.MessagesToString(), Is.Not.Empty);
            }
        }
    }
}
