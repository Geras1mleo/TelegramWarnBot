namespace TelegramWarnBot;

public class ConfigurationContext : IOContext
{
    private BotConfiguration botConfiguration;
    private Configuration configuration;
    private Trigger[] triggers;
    private IllegalTrigger[] illegalTriggers;

    public BotConfiguration BotConfiguration
    {
        get
        {
            if (botConfiguration is null)
            {
                botConfiguration = Deserialize<BotConfiguration>("Bot.json");
                botConfiguration.RegisteredChats = Deserialize<List<long>>(Path.Combine("Configuration", "RegisteredChats.json"));
            }

            return botConfiguration;
        }
    }

    public Configuration Configuration
    {
        get
        {
            if (configuration is null)
                configuration = Deserialize<Configuration>(Path.Combine("Configuration", "Configuration.json"));

            return configuration;
        }
    }

    public Trigger[] Triggers
    {
        get
        {
            if (triggers is null)
                triggers = Deserialize<Trigger[]>(Path.Combine("Configuration", "Triggers.json"));

            return triggers;
        }
    }

    public IllegalTrigger[] IllegalTriggers
    {
        get
        {
            if (illegalTriggers is null)
                illegalTriggers = Deserialize<IllegalTrigger[]>(Path.Combine("Configuration", "IllegalTriggers.json"));

            return illegalTriggers;
        }
    }

    public void ReloadConfiguration()
    {
        configuration = null;
        triggers = null;
        illegalTriggers = null;
    }

    public bool IsChatRegistered(long chatId)
    {
        return BotConfiguration.RegisteredChats.Any(c => c == chatId);
    }
}
