namespace TelegramWarnBot;

public abstract class Pipe<TContext>
    where TContext : IContext
{
    protected readonly Func<TContext, Task> next;

    protected Pipe(Func<TContext, Task> next)
    {
        this.next = next;
    }

    public abstract Task Handle(TContext context);
}

public class PipeBuilder<TContext>
    where TContext : IContext
{
    private readonly Func<TContext, Task> mainAction;
    private readonly List<PipeContainer> pipes;
    private readonly IServiceProvider provider;

    public PipeBuilder(Func<TContext, Task> mainAction, IServiceProvider provider)
    {
        this.mainAction = mainAction;
        this.provider = provider;
        pipes = new List<PipeContainer>();
    }

    public PipeBuilder<TContext> AddPipe<T>()
        where T : Pipe<TContext>
    {
        return AddPipe<T>(_ => true);
    }

    public PipeBuilder<TContext> AddPipe<T>(Func<TContext, bool> executionFilter)
        where T : Pipe<TContext>
    {
        pipes.Add(new PipeContainer
        {
            Type = typeof(T),
            ExecutionFilter = executionFilter
        });
        return this;
    }

    public Func<TContext, Task> Build()
    {
        return CreatePipe(0);
    }

    public Func<TContext, Task> CreatePipe(int index)
    {
        if (index < pipes.Count - 1)
        {
            var child = CreatePipe(index + 1);
            var pipe = (Pipe<TContext>)Activator.CreateInstance(pipes[index].Type, ResolveDependencies(child, pipes[index].Type))!;
            return context =>
            {
                if (context.ResolveAttributes(pipes[index].Type) && pipes[index].ExecutionFilter(context))
                    return pipe.Handle(context);

                return child(context);
            };
        }

        var finalPipe = (Pipe<TContext>)Activator.CreateInstance(pipes[index].Type, ResolveDependencies(mainAction, pipes[index].Type))!;
        return context =>
        {
            if (context.ResolveAttributes(pipes[index].Type) && pipes[index].ExecutionFilter(context))
                return finalPipe.Handle(context);

            return mainAction(context);
        };
    }

    private object[] ResolveDependencies(object firstParam, Type type)
    {
        var parameters = new List<object>
        {
            firstParam
        };

        var ctor = type.GetConstructors()[0];

        foreach (var param in ctor.GetParameters().Skip(1))
            parameters.Add(provider.GetService(param.ParameterType));

        return parameters.ToArray();
    }

    private class PipeContainer
    {
        public Type Type { get; init; }
        public Func<TContext, bool> ExecutionFilter { get; init; }
    }
}