using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Tests.Odin.Email
{
    public sealed class Program
    {
        public static WebApplication BuildApplication(string[]? args = null)
        {
            string baseDir = AppContext.BaseDirectory;
            WebApplicationOptions appOptions = new WebApplicationOptions()
            {
                Args = args ?? [],
                ContentRootPath = baseDir
            };
            WebApplicationBuilder builder = WebApplication.CreateBuilder(appOptions);
            builder.Configuration.AddJsonFile("appSettings.json", false);
            builder.Configuration.AddUserSecrets<Program>();
            return builder.Build();
        }
    }
}
