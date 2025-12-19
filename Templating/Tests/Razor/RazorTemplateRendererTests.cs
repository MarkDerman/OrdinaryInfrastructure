using System.Reflection;
using NUnit.Framework;
using Odin.System;
using Odin.Templating;

namespace Tests.Odin.Templating.Razor
{
    [TestFixture]
    public sealed class RazorTemplateRendererTests
    {

        [Test]
        [TestCase("Tests.Odin.Templating", "TestTemplate2", true, Description = "Should work. Without a period")]
        [TestCase("Tests.Odin.Templating.Views", "TestTemplate1", true, Description = "Should work. Without a period")]
        [TestCase("tests.odin.templating.views", "TestTemplate1", false, Description = "Case sensitive namespace..")]
        [TestCase("Tests.Odin.Templating.Views.", "TestTemplate1", true, Description = "Should work. With a period")]
        [TestCase("tests.odin.templating.razor", "testtemplate1", false, Description = "Case sensitive template")]
        [TestCase("Wrong", "TestTemplate1", false, Description = "Wrong namespace")]
        [TestCase("Tests.Odin.Templating.Views", "Wrong", false, Description = "Wrong templateKey")]
        [TestCase("Tests.Odin.Templating", "TestTemplate1", false, Description = "Testing sub dirs - they don't work")]
        [TestCase(null, "TestTemplate1", false, Description = "Testing optionality of namespace. Doesn't work.")]
        [TestCase("", "TestTemplate1", false, Description = "Testing optionality of namespace. Doesn't work.")]
        public async Task Create_with_different_templateKeys_and_namespaces(string rootNamespace, string templateKey, bool shouldSucceed)
        {
            Assembly testsAssembly = typeof(RazorTemplateRendererTests).Assembly;
            RazorTemplateRenderer sut = new RazorTemplateRenderer(testsAssembly, rootNamespace);
            ResultValue<string> result = await sut.RenderAsync(templateKey, new TestViewModel(){ Title = "World"});

            Assert.That(result.IsSuccess, Is.EqualTo(shouldSucceed));
            if (shouldSucceed)
            {
                Assert.That(result.Value, Does.Contain("<div>Hello World</div>"), result.MessagesToString());
            }
            else
            {
                Assert.That(result.MessagesToString(), Is.Not.Empty);
            }
        }

    }
}