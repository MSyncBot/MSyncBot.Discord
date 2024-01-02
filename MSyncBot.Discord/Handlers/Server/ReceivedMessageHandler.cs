using System.Text;
using System.Text.Json;
using DSharpPlus.Entities;
using MSyncBot.Types;
using MSyncBot.Types.Enums;

namespace MSyncBot.Discord.Handlers.Server;

public class ReceivedMessageHandler
{
    public static ulong LastUserId;

    public void ReceiveMessage(byte[] buffer, long offset, long size) =>
        _ = Task.Run(async () =>
        {
            var jsonMessage = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            var message = JsonSerializer.Deserialize<Message>(jsonMessage);

            if (message.Messenger.Type is MessengerType.Discord)
                return;

            var guild = await Bot.Client.GetGuildAsync(645297558994026513);
            var channel = guild.GetChannel(1054416672808775730);

            var embed = LastUserId != message.User.Id
                ? new DiscordEmbedBuilder
                    {
                        Title = message.Messenger.Type.ToString(),
                        Color = DiscordColor.Azure,
                        Description = message.Text ?? "null",
                    }
                    .WithAuthor(name: $"{message.User.FirstName} {message.User.LastName}")
                    .WithTimestamp(DateTime.Now)
                : new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Azure,
                    Description = message.Text ?? "null",
                };

            switch (message.Type)
            {
                case MessageType.Text:
                    Bot.Logger.LogInformation(
                        $"Received message from {message.Messenger.Name}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - {message.Text}");

                    await channel.SendMessageAsync(embed.Build());
                    break;

                default:
                {
                    Bot.Logger.LogInformation(
                        $"Received album from {message.Messenger.Name} with {message.Files.Count} files: " +
                        $"{message.User.FirstName} ({message.User.Id})");

                    var embedMessage = await channel.SendMessageAsync(embed.Build());
                    
                    var messageBuilder = new DiscordMessageBuilder()
                        .WithReply(embedMessage.Id);

                    foreach (var file in message.Files)
                    {
                        var memoryStream = new MemoryStream(file.Data);
                        messageBuilder.AddFile($"{file.Name}{file.Extension}", memoryStream);
                    }
                    
                    await channel.SendMessageAsync(messageBuilder);

                    break;
                } 
            }
            
            LastUserId = message.User.Id;
        });
}