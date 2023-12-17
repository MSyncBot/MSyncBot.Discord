using DSharpPlus;
using DSharpPlus.EventArgs;

namespace MSyncBot.Discord.Handlers;

public class ErrorHandler
{
    public static Task GetError(DiscordClient sender, ClientErrorEventArgs e)
    {
        Bot.Logger.LogError(e.Exception.Message);
        return Task.CompletedTask;
    }
}