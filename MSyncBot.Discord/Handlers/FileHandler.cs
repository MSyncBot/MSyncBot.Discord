using DSharpPlus.Entities;
using MSyncBot.Types;
using MSyncBot.Types.Enums;
using File = MSyncBot.Types.File;

namespace MSyncBot.Discord.Handlers;

public class FileHandler
{
    public async Task<File?> DownloadFileAsync(DiscordAttachment attachment)
    {
        try
        {
            using var httpClient = new HttpClient();
            var fileData = await httpClient.GetByteArrayAsync(new Uri(attachment.Url));
            var fileName = Guid.NewGuid().ToString();
            var fileExtension = Path.GetExtension(attachment.FileName);

            var mediaType = !string.IsNullOrEmpty(attachment.MediaType)
                ? attachment.MediaType.Split('/')[0] 
                : "document";

            var fileType = mediaType switch
            {
                "image" => FileType.Photo,
                "video" => FileType.Video,
                "audio" => FileType.Audio,
                _ => FileType.Document
            };

            return new File(fileName, fileExtension, fileData, fileType);
        }
        catch (Exception ex)
        {
            Bot.Logger.LogError(ex.ToString());
        }

        return null;
    }
}