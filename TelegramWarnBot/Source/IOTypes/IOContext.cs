namespace TelegramWarnBot;

public abstract class IOContext
{
    private readonly IHostEnvironment hostEnvironment;

    public IOContext(IHostEnvironment hostEnvironment)
    {
        this.hostEnvironment = hostEnvironment;
    }

    protected T Deserialize<T>(string path)
    {
        var text = System.IO.File.ReadAllText(Path.Combine(hostEnvironment.ContentRootPath, path));
        return JsonConvert.DeserializeObject<T>(text) ?? throw new Exception($"U fucker changed {path} file...");
    }

    protected Task SerializeAsync(object value, string path)
    {
        var text = JsonConvert.SerializeObject(value, Formatting.Indented);
        return System.IO.File.WriteAllTextAsync(Path.Combine(hostEnvironment.ContentRootPath, path), text, Encoding.UTF8);
    }
}