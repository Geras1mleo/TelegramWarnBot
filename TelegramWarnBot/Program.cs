
Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

var cts = new CancellationTokenSource();

var host = AppConfiguration.Build();

var bot = ActivatorUtilities.GetServiceOrCreateInstance<IBot>(host.Services);

var consoleHandler = ActivatorUtilities.GetServiceOrCreateInstance<IConsoleCommandHandler>(host.Services);

var logger = ActivatorUtilities.GetServiceOrCreateInstance<ILogger<Program>>(host.Services);

var cachedDataContext = ActivatorUtilities.GetServiceOrCreateInstance<ICachedDataContext>(host.Services);

bot.Run(host.Services, cts.Token);

consoleHandler.Start(cts.Token);

await host.RunAsync(cts.Token);

logger.LogInformation("Saving data...");

cachedDataContext.SaveData();

logger.LogInformation("Data saved successfully!");