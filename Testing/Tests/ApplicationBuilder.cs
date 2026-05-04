using Microsoft.Extensions.Hosting;

namespace Tests.Odin.Testing
{
    public static class ApplicationBuilder
    {
        public static HostApplicationBuilder CreateBuilder()
        {
            HostApplicationBuilder hostBuilder = Host.CreateApplicationBuilder();
            return hostBuilder;
        }
    }
}