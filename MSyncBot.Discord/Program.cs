using MConfiguration;
using MLoggerService;

namespace MSyncBot.Discord;

internal class Program
{
    private static async Task Main()
    {
        var logger = new MLogger();
        logger.LogProcess("Logger initializing...");
        logger.LogSuccess("Logger successfully initialized.");

        var config = new ConfigManager();
        var modelConfig = new ModelConfig();

        foreach (var property in typeof(ModelConfig).GetProperties())
        {
            var propertyName = property.Name;
            var data = config.Get(propertyName);

            if (string.IsNullOrEmpty(data))
            {
                logger.LogInformation($"Enter value for {propertyName}:");
                var value = Console.ReadLine();
                property.SetValue(modelConfig, Convert.ChangeType(value, property.PropertyType));
                continue;
            }

            property.SetValue(modelConfig, Convert.ChangeType(data, property.PropertyType));
        }
        
        config.Set(modelConfig);

        var bot = new Bot(
            token: modelConfig.BotToken,
            logger: logger,
            database: new MDatabase.MDatabase(
                modelConfig.DatabaseIp,
                modelConfig.DatabaseName,
                modelConfig.DatabaseUser,
                modelConfig.DatabaseUserPassword
                ),
            serverIp: modelConfig.ServerIp,
            serverPort: modelConfig.ServerPort
        );
        await bot.StartAsync();
        await Task.Delay(-1);
    }
}