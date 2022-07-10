namespace TelegramWarnBot;

public abstract class Pipe<T>
{
    protected Func<T, Task> next;
    protected Pipe(Func<T, Task> next)
    {
        this.next = next;
    }

    public abstract Task Handle(T context);
}

public class PipeBuilder<T>
{
    private readonly ILifetimeScope scope;
    private readonly Func<T, Task> mainAction;
    private readonly List<PipeContainer> pipes;

    public PipeBuilder(Func<T, Task> mainAction, ILifetimeScope scope)
    {
        this.mainAction = mainAction;
        this.scope = scope;
        pipes = new List<PipeContainer>();
    }

    public PipeBuilder<T> AddPipe<Type>()
        where Type : Pipe<T>
    {
        return AddPipe<Type>(_ => true);
    }

    public PipeBuilder<T> AddPipe<Type>(Func<T, bool> executionFilter)
        where Type : Pipe<T>
    {
        pipes.Add(new()
        {
            Type = typeof(Type),
            ExecutionFilter = executionFilter
        });
        return this;
    }

    public Func<T, Task> Build()
    {
        return CreatePipe(0);
    }

    public Func<T, Task> CreatePipe(int index)
    {
        if (index < pipes.Count - 1)
        {
            var child = CreatePipe(index + 1);
            var pipe = (Pipe<T>)Activator.CreateInstance(pipes[index].Type, ResolveDependencies(child, pipes[index].Type));

            return context =>
            {
                if (pipes[index].ExecutionFilter(context))
                    return pipe.Handle(context);
                else
                    return child(context);
            };
        }
        else
        {
            var finalPipe = (Pipe<T>)Activator.CreateInstance(pipes[index].Type, ResolveDependencies(mainAction, pipes[index].Type));
            return context =>
            {
                if (pipes[index].ExecutionFilter(context))
                    return finalPipe.Handle(context);
                else
                    return mainAction(context);
            };
        }
    }

    private object[] ResolveDependencies(object firstParam, Type type)
    {
        var parameters = new List<object>()
        {
            firstParam
        };

        var ctor = type.GetConstructors()[0];

        foreach (var param in ctor.GetParameters().Skip(1))
        {
            parameters.Add(scope.Resolve(param.ParameterType));
        }

        return parameters.ToArray();
    }

    public class PipeContainer
    {
        public Type Type { get; init; }
        public Func<T, bool> ExecutionFilter { get; init; }
    }
}