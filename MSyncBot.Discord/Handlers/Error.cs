using DSharpPlus;
using DSharpPlus.EventArgs;

namespace MSyncBot.Discord.Handlers;

public class Error
{
    public static Task ErrorHandler(DiscordClient sender, ClientErrorEventArgs e)
    {
        Bot.Logger.LogError(e.Exception.Message);
        return Task.CompletedTask;
    }   
}