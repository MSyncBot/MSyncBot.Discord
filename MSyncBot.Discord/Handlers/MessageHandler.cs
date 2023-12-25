using System.Net;
using System.Text.Json;
using DSharpPlus;
using DSharpPlus.EventArgs;
using MSyncBot.Types;
using MSyncBot.Types.Enums;
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

            var attachments = mc.Message.Attachments;
            switch (attachments.Count)
            {
                case > 0 and 1:
                    var attachment = attachments.FirstOrDefault();
                    var downloadedFile = await new FileHandler()
                        .DownloadFileAsync(attachment);

                    var messageType = downloadedFile.FileType switch
                    {
                        FileType.Photo => MessageType.Photo,
                        FileType.Video => MessageType.Video,
                        FileType.Audio => MessageType.Audio,
                        _ => MessageType.Document,
                    };
                    
                    var fileMessage = new Message("MSyncBot.Discord",
                        2,
                        SenderType.Discord,
                        messageType,
                        new User(user.Username, id: user.Id));
                    fileMessage.MediaFiles.Add(downloadedFile);
                    fileMessage.Content = mc.Message.Content;
                
                    var fileJsonMessage = JsonSerializer.Serialize(fileMessage);
                    Bot.Server.SendTextAsync(fileJsonMessage);
                    return;
                case > 0:
                {
                    var downloadFilesTasks = new List<Task<MediaFile?>>();
                    downloadFilesTasks.AddRange(attachments.Select(attachment =>
                        new MediaFileHandler().DownloadFileAsync(attachment)));
                    var downloadedFiles = await Task.WhenAll(downloadFilesTasks);
                
                    var albumMessage = new Message("MSyncBot.Discord",
                        2,
                        SenderType.Discord,
                        MessageType.Album,
                        new User(user.Username, id: user.Id));
                    albumMessage.MediaFiles.AddRange(downloadedFiles!);
                    albumMessage.Content = mc.Message.Content;
                
                    var albumJsonMessage = JsonSerializer.Serialize(albumMessage);
                    Bot.Server.SendTextAsync(albumJsonMessage);
                    return;
                }
            }
            
            var textMessage = new Message("MSyncBot.Discord",
                2,
                SenderType.Discord,
                MessageType.Text,
                new User(user.Username, id: user.Id)
            )
            {
                Content = mc.Message.Content
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