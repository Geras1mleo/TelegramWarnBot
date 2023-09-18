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
            .ConfigureAppConfiguration(ConfigureAppConfiguration)
            .ConfigureServices(ConfigureServices)
            .UseConsoleLifetime()
            .UseSerilog()
            .Build();

        var env = host.Services.GetService<IHostEnvironment>();
        var envPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Windows";

        Log.Logger.Information("Hosting platform: {platform}", envPlatform);

        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables();

        // Configuring logger again with provided appsettings
        var configurationRoot = builder.Build();

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.SerilogTelegramSink(configurationRoot, LogEventLevel.Warning)
            .WriteTo.SerilogTelegramInfoSink(configurationRoot, LogEventLevel.Verbose)
            .ReadFrom.Configuration(configurationRoot)
            .CreateLogger();

        Log.Logger.Information("Configured successfully!");

        return host;
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        // To use dotnet watch/run and not corrupt Data\ files in project
        var env = context.HostingEnvironment;
        if (env.IsDevelopment() && env.ContentRootPath.EndsWith(env.ApplicationName))
            env.ContentRootPath += Path.DirectorySeparatorChar + Path.Combine("bin", "Debug", "net6.0");
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IBot, Bot>();
        services.AddSingleton<ITelegramBotClientProvider, TelegramBotClientProvider>();

        services.AddSingleton<IConfigurationContext, ConfigurationContext>();
        services.AddSingleton<ICachedDataContext, CachedDataContext>();
        services.AddSingleton<IInMemoryCachedDataContext, InMemoryCachedDataContext>();

        services.AddSingleton<IStatsController, StatsController>();

        services.AddTransient<IConsoleCommandHandler, ConsoleCommandHandler>();
        services.AddTransient<ICommand, RegisterCommand>();
        services.AddTransient<ICommand, SendCommand>();
        services.AddTransient<ICommand, InfoCommand>();
        services.AddTransient<ICommand, ReloadCommand>();
        services.AddTransient<ICommand, LeaveCommand>();
        services.AddTransient<ICommand, SaveCommand>();
        services.AddTransient<ICommand, VersionCommand>();
        services.AddTransient<ICommand, ClearCommand>();

        services.AddTransient<IUpdateContextBuilder, UpdateContextBuilder>();

        services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        services.AddTransient<ICommandsController, CommandsController>();
        services.AddTransient<IMessageHelper, MessageHelper>();
        services.AddTransient<IChatHelper, ChatHelper>();
        services.AddTransient<ICommandService, CommandService>();
        services.AddTransient<IResponseHelper, ResponseHelper>();

        services.AddSmartFormatterProvider();
    }

    public static PipeBuilder<UpdateContext> GetPipeBuilder(IServiceProvider provider)
    {
        return new PipeBuilder<UpdateContext>(_ => Task.CompletedTask, provider)
            .AddPipe<CachingHandler>(c => c.IsMessageUpdate)
            .AddPipe<JoinedLeftHandler>(c => c.IsJoinedLeftUpdate)
            .AddPipe<AdminsHandler>(c => c.IsAdminsUpdate)
            .AddPipe<SpamHandler>(c => !c.IsSenderAdmin && c.IsMessageUpdate) // TODO attribute
            .AddPipe<TriggersHandler>()
            .AddPipe<IllegalTriggersHandler>()
            .AddPipe<CommandHandler>(c => c.IsCommandUpdate);
    }
}