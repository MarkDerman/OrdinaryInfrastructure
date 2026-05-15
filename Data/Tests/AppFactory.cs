using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Tests.Odin.Data
{
    public class AppFactory : WebApplicationFactory<Program>
    { 
        // Nothing needed yet...
        // protected override void ConfigureWebHost(IWebHostBuilder builder)
        // {
        //     builder.ConfigureAppConfiguration((context, config) =>
        //     {
        //         // Add a specific test settings file if needed
        //         config.AddJsonFile("appsettings.Test.json", optional: true);
        //     });
        //
        //     builder.ConfigureTestServices(services =>
        //     {
        //         // 1. Remove the real database registration
        //         // 2. Add a Mock or an In-Memory/Testcontainer version
        //         // Example: services.AddDbContext<MyDbContext>(options => options.UseInMemoryDatabase("TestDb"));
        //     
        //         // You can also mock external APIs here using Moq or NSubstitute
        //     });
        //
        //     builder.UseEnvironment("Testing");
        // }
        //
        // public override async ValueTask DisposeAsync()
        // {
        // }
    }
}