using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Hosting;

using IHost host = Host.CreateApplicationBuilder(args)

    .ConfigureServices((context, services) =>

    {

        services.AddSingleton<IMyService, MyService>();

        services.AddTransient<App>();

    })

    .Build();