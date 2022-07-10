﻿using Autofac;

Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

var cts = new CancellationTokenSource();

var container = DependenciesContainerConfig.Configure();

using (var scope = container.BeginLifetimeScope())
{
    // Makes sure all data is saved when closing console, also cancelling token and breaks all running requests etc..
    scope.Resolve<CloseHandler>().Configure(cts);

    var bot = scope.Resolve<Bot>().Start(scope, cts.Token);

    scope.Resolve<ConsoleCommandHandler>().Start(cts.Token);
}
