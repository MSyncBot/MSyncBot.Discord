using MLoggerService;

namespace MSyncBot.Discord;

class Program
{
    private static async Task Main()
    {
        var bot = new Bot("####",
            new MLogger(),
            new MDatabase.MDatabase("####", "####", "####", "####"));
        await bot.StartAsync();
        await Task.Delay(-1);
    }
}