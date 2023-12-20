using MLoggerService;

namespace MSyncBot.Discord;

internal class Program
{
    private static async Task Main()
    {
        var bot = new Bot("MTE4Mjc4NDE5MzA3MjIwOTk0MA.GJtdBD.nWe5YGDjoA3d8x957zzRkF1xYd1JYHKEToKt7k",
            new MLogger(),
            new MDatabase.MDatabase("####", "####", "####", "####"));
        await bot.StartAsync();
        await Task.Delay(-1);
    }
}