namespace TelegramWarnBot;

public static class AppConfiguration
{
    public static IHost Build()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger.Information("Configuring and building host...");

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) =>
            {
                // To use dotnet watch/run and not corrupt Data\ files in project
                var env = context.HostingEnvironment;
                if (env.IsDevelopment()
                 && env.ContentRootPath.EndsWith(env.ApplicationName))
                {
                    env.ContentRootPath += @"\bin\Debug\net6.0";
                }
            })
            .ConfigureServices(ConfigureServices)
            .UseConsoleLifetime()
            .UseSerilog()
            .Build();

        var env = host.Services.GetService<IHostEnvironment>();

        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(new JsonFormatter(), Path.Combine("Data", "Logs.json"), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .CreateLogger();

        Log.Logger.Information("Configured successfully!");

        return host;
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IBot, Bot>();

        services.AddSingleton<IConfigurationContext, ConfigurationContext>();
        services.AddSingleton<ICachedDataContext, CachedDataContext>();

        services.AddTransient<IConsoleCommandHandler, ConsoleCommandHandler>();

        services.AddTransient<IWarnController, WarnController>();
        services.AddTransient<IMessageHelper, MessageHelper>();
        services.AddTransient<IChatHelper, ChatHelper>();
        services.AddTransient<ICommandService, CommandService>();
        services.AddTransient<IResponseHelper, ResponseHelper>();
    }
}
