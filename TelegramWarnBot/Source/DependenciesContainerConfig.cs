using Autofac;

namespace TelegramWarnBot;

public static class DependenciesContainerConfig
{
    public static IContainer Configure()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<Bot>().As<IBot>();
        builder.RegisterType<CloseHandler>().As<ICloseHandler>();
        builder.RegisterType<ConsoleCommandHandler>().As<IConsoleCommandHandler>();

        builder.RegisterType<ConfigurationContext>().As<IConfigurationContext>().InstancePerLifetimeScope();
        builder.RegisterType<CachedDataContext>().As<ICachedDataContext>().InstancePerLifetimeScope();

        builder.RegisterType<WarnController>().As<IWarnController>();

        builder.RegisterType<MessageHelper>().As<IMessageHelper>();
        builder.RegisterType<ChatHelper>().As<IChatHelper>();
        builder.RegisterType<CommandService>().As<ICommandService>();
        builder.RegisterType<ResponseHelper>().As<IResponseHelper>();

        return builder.Build();
    }
}
