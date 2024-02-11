using System.Net;
using System.Text.Json;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MSyncBot.Discord.Handlers.Server;
using MSyncBot.Types;
using MSyncBot.Types.Enums;
using File = MSyncBot.Types.File;
using MessageType = MSyncBot.Types.Enums.MessageType;

namespace MSyncBot.Discord.Handlers;

public class MessageHandler
{
    public static async Task MessageCreated(DiscordClient client, MessageCreateEventArgs mc)
    {
        try
        {
            var user = mc.Author;
            if (user.Id == Bot.Client.CurrentUser.Id)
                return;

            if (mc.Channel.Id != 1054416672808775730)
                return;

            ReceivedMessageHandler.LastUserId = mc.Author.Id;
            
            var attachments = mc.Message.Attachments;
            switch (attachments.Count)
            {
                case > 0 and 1:
                    var attachment = attachments.FirstOrDefault();
                    var downloadedFile = await new FileHandler()
                        .DownloadFileAsync(attachment);

                    var messageType = downloadedFile.Type switch
                    {
                        FileType.Photo => MessageType.Photo,
                        FileType.Video => MessageType.Video,
                        FileType.Audio => MessageType.Audio,
                        _ => MessageType.Document,
                    };

                    var fileMessage = new Message(
                        new Messenger("MSyncBot.Discord", MessengerType.Discord),
                        messageType,
                        new User(mc.Author.Username, mc.Author.Id),
                        new Chat(mc.Channel.Name, mc.Channel.Id)
                    );
                    fileMessage.Files.Add(downloadedFile);
                    fileMessage.Text = mc.Message.Content;
                
                    var fileJsonMessage = JsonSerializer.Serialize(fileMessage);
                    Bot.Server.SendTextAsync(fileJsonMessage);
                    return;
                case > 0:
                {
                    var downloadFilesTasks = new List<Task<File?>>();
                    downloadFilesTasks.AddRange(attachments.Select(attachment =>
                        new FileHandler().DownloadFileAsync(attachment)));
                    var downloadedFiles = await Task.WhenAll(downloadFilesTasks);

                    var albumMessage = new Message(
                        new Messenger("MSyncBot.Discord", MessengerType.Discord),
                        MessageType.Album,
                        new User(mc.Author.Username, mc.Author.Id),
                        new Chat(mc.Channel.Name, mc.Channel.Id)
                    );
                    albumMessage.Files.AddRange(downloadedFiles!);
                    albumMessage.Text = mc.Message.Content;
                
                    var albumJsonMessage = JsonSerializer.Serialize(albumMessage);
                    Bot.Server.SendTextAsync(albumJsonMessage);
                    return;
                }
            }

            var textMessage = new Message(
                new Messenger("MSyncBot.Discord", MessengerType.Discord),
                MessageType.Text,
                new User(mc.Author.Username, mc.Author.Id),
                new Chat(mc.Channel.Name, mc.Channel.Id)
            )
            {
                Text = mc.Message.Content
            };

            var jsonTextMessage = JsonSerializer.Serialize(textMessage);
            Bot.Server.SendTextAsync(jsonTextMessage);
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.Message);
        }
    }
}