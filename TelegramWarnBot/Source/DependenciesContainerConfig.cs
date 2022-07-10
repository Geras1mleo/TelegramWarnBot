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

        builder.RegisterType<UpdateHelper>().As<UpdateHelper>();
        builder.RegisterType<ChatService>().As<ChatService>();
        builder.RegisterType<ResponseHelper>().As<ResponseHelper>();

        return builder.Build();
    }
}
