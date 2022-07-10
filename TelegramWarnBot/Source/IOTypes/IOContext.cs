namespace TelegramWarnBot;

public class IOContext
{
    private static readonly string ExecutablePath = AppDomain.CurrentDomain.BaseDirectory;

    protected T Deserialize<T>(string path)
    {
        var text = System.IO.File.ReadAllText(Path.Combine(ExecutablePath, path));
        return JsonConvert.DeserializeObject<T>(text) ?? throw new Exception($"U fucker changed {path} file...");
    }

    protected Task SerializeAsync(object value, string path)
    {
        var text = JsonConvert.SerializeObject(value, Formatting.Indented);
        return System.IO.File.WriteAllTextAsync(Path.Combine(ExecutablePath, path), text, Encoding.UTF8);
    }
}