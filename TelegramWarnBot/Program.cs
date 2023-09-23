Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

var cts = new CancellationTokenSource();

var host = AppConfiguration.Build();

var bot = host.Services.GetService<IBot>();

var consoleHandler = host.Services.GetService<IConsoleCommandHandler>();

var logger = host.Services.GetService<ILogger<Program>>();

var cachedDataContext = host.Services.GetService<ICachedDataContext>();

await bot.StartAsync(cts.Token);

consoleHandler.Start(cts.Token);

await host.RunAsync(cts.Token);

logger.LogInformation("Saving data...");

cachedDataContext.SaveData();

logger.LogInformation("Data saved successfully!");

// TODO: Ukrainian Roulette
// spin for extra warning or forgiveness
// /spin command

// TODO: Skip <count> updates on start of bot, use IOptions and CommandLine pkg

// TODO: Warnings list: Save admin id that gave warning, date/time, optional reason