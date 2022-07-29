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
            .UseSmartFormatter()
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
            .WriteTo.File(new JsonFormatter(),
                          Path.Combine(env.ContentRootPath, "Data", "Logs.json"),
                          restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
            .CreateLogger();

        Log.Logger.Information("Configured successfully!");

        return host;
    }

    private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
    {
        // To use dotnet watch/run and not corrupt Data\ files in project
        var env = context.HostingEnvironment;
        if (env.IsDevelopment() && env.ContentRootPath.EndsWith(env.ApplicationName))
        {
            env.ContentRootPath += @"\bin\Debug\net6.0";
        }
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.AddSingleton<IBot, Bot>();

        services.AddSingleton<IConfigurationContext, ConfigurationContext>();
        services.AddSingleton<ICachedDataContext, CachedDataContext>();

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
    }

    public static PipeBuilder<UpdateContext> GetPipeBuilder(IServiceProvider provider)
    {
        return new PipeBuilder<UpdateContext>(_ => Task.CompletedTask, provider)
                    .AddPipe<JoinedLeftHandler>(c => c.IsJoinedLeftUpdate)
                    .AddPipe<CachingHandler>(c => c.IsMessageUpdate)
                    .AddPipe<AdminsHandler>(c => c.IsAdminsUpdate)
                    .AddPipe<SpamHandler>(c => c.IsBotAdmin && !c.IsSenderAdmin)
                    .AddPipe<TriggersHandler>()
                    .AddPipe<IllegalTriggersHandler>(c => c.IsBotAdmin)
                    .AddPipe<CommandHandler>(c => c.Update.Message.Text.IsValidCommand());
    }
}
