using Autofac;

namespace TelegramWarnBot;

public static class DependenciesContainerConfig
{
    public static IContainer Configure()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<Bot>().As<Bot>();
        builder.RegisterType<CloseHandler>().As<CloseHandler>();
        builder.RegisterType<ConsoleCommandHandler>().As<ConsoleCommandHandler>();

        builder.RegisterType<ConfigurationContext>().As<ConfigurationContext>().InstancePerLifetimeScope();
        builder.RegisterType<CachedDataContext>().As<CachedDataContext>().InstancePerLifetimeScope();

        builder.RegisterType<WarnController>().As<WarnController>();

        builder.RegisterType<MessageHelper>().As<MessageHelper>();
        builder.RegisterType<ChatHelper>().As<ChatHelper>();
        builder.RegisterType<CommandService>().As<CommandService>();
        builder.RegisterType<ResponseHelper>().As<ResponseHelper>();

        return builder.Build();
    }
}
