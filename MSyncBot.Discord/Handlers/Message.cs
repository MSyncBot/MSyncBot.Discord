using DSharpPlus;
using DSharpPlus.EventArgs;
using MSyncBot.Discord.Handlers.Server;

namespace MSyncBot.Discord.Handlers;

public class Message
{
    public static async Task MessageCreatedHandler(DiscordClient client, MessageCreateEventArgs mc)
    {
        try
        {
            if (mc.Author.Id == Bot.Client.CurrentUser.Id)
                return;
            Bot.Logger.LogInformation($"{mc.Author.Username} ({mc.Author.Id}) send message: {mc.Message.Content}");
            await Bot.Server.SendMessageAsync(Bot.Server.TcpClient.GetStream(), 
                new Client("MSyncBot.Discord", ClientType.Discord, message: mc.Message.Content));
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }
    }
}