
var cts = new CancellationTokenSource();

Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

// Makes sure all data is saved when closing console, also cancelling token and breaks all running requests etc..
CloseHandler.Configure(cts);

Bot.Start(cts.Token);

CommandHandler.StartListening(cts.Token);