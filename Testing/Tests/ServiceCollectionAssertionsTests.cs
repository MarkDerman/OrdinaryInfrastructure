using Microsoft.Extensions.DependencyInjection;
using Odin.Testing;
using Xunit;

namespace Tests.Odin.Testing
{
    public sealed class ServiceCollectionGenericAssertionsTests
    {
        [Theory]
        [MemberData(nameof(GetAllTestFrameworkAdaptors))]
        public void Assert_by_type_and_lifetime(IAssertionAdaptor adaptor)
        {
            var appBuilder = ApplicationBuilder.CreateBuilder();
            appBuilder.Services.AddTransient<IFakeService, FakeService>();
            var app = appBuilder.Build();

            ServiceCollectionGenericAssertions.AssertServiceRegistration(
                app.Services, adaptor, typeof(IFakeService), ServiceLifetime.Transient, 1);
        }

        public static IReadOnlyList<object[]> GetAllTestFrameworkAdaptors()
        {
            return Adaptors.GetAllTestFrameworkAdaptors().AsSingleObjectsList();
            return new List<object[]>
            {
                new[] { new Odin.Testing.XUnit.AssertionAdaptor() },
                new[] { new Odin.Testing.NUnit.AssertionAdaptor() },
                new[] { new Odin.Testing.XUnitV2.AssertionAdaptor() }
            };
        }
    }
}