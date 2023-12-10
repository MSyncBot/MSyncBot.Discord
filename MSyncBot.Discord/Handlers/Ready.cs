using DSharpPlus;
using DSharpPlus.EventArgs;

namespace MSyncBot.Discord.Handlers;

public static class Ready
{
    public static Task ReadyHandler(DiscordClient sender, ReadyEventArgs e)
    {
        Bot.Logger.LogSuccess("Bot is ready to receive events.");
        return Task.CompletedTask;
    }
}