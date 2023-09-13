using File = System.IO.File;

namespace TelegramWarnBot;

public abstract class IOContextBase
{
    private readonly IHostEnvironment hostEnvironment;

    public IOContextBase(IHostEnvironment hostEnvironment)
    {
        this.hostEnvironment = hostEnvironment;
    }

    protected T Deserialize<T>(string path)
    {
        var text = File.ReadAllText(Path.Combine(hostEnvironment.ContentRootPath, path));
        return JsonConvert.DeserializeObject<T>(text) ?? throw new Exception($"Something went wrong with file {path}");
    }

    protected Task SerializeAsync(object value, string path)
    {
        var text = JsonConvert.SerializeObject(value, Formatting.Indented);
        return File.WriteAllTextAsync(Path.Combine(hostEnvironment.ContentRootPath, path), text, Encoding.UTF8);
    }
}