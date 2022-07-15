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
