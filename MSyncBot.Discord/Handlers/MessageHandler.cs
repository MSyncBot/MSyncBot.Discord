using System.Net;
using System.Text.Json;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MSyncBot.Discord.Handlers.Server.Types;
using MSyncBot.Discord.Handlers.Server.Types.Enums;
using MessageType = MSyncBot.Discord.Handlers.Server.Types.Enums.MessageType;

namespace MSyncBot.Discord.Handlers;

public class MessageHandler
{
    public static async Task MessageCreated(DiscordClient client, MessageCreateEventArgs mc)
    {
        try
        {
            if (mc.Author.Id == Bot.Client.CurrentUser.Id)
                return;

            if (mc.Message.Attachments.Count > 0)
            {
                foreach (var attachment in mc.Message.Attachments)
                {
                    if (!attachment.MediaType.StartsWith("image"))
                        continue;

                    using var webClient = new WebClient();
                    var photoBytes = await webClient.DownloadDataTaskAsync(new Uri(attachment.Url));

                    var photoName = Guid.NewGuid().ToString();
                    const string extension = ".png";

                    var mediaFile = new MediaFile(photoName, extension, photoBytes, FileType.Photo);
                    var photoMessage = new Message("MSyncBot.Discord",
                        2,
                        SenderType.Discord,
                        MessageType.Photo,
                        new User(mc.Author.Username));
                    photoMessage.MediaFiles.Add(mediaFile);

                    var jsonPhotoMessage = JsonSerializer.Serialize(photoMessage);
                    Bot.Server.SendTextAsync(jsonPhotoMessage);
                }

                return;
            }

            var multicastMessage = new Message("MSyncBot.Discord",
                2,
                SenderType.Discord,
                MessageType.Text,
                new User("Discord")
                )
            {
                Content = mc.Message.Content
            };
            var jsonMessage = JsonSerializer.Serialize(multicastMessage);
            Bot.Server.SendTextAsync(jsonMessage);
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }
    }
}