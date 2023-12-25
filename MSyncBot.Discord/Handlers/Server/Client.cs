using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DSharpPlus.Entities;
using MSyncBot.Types;
using MSyncBot.Types.Enums;
using NetCoreServer;

namespace MSyncBot.Discord.Handlers.Server;

public class Client : WsClient
{
    private bool _stop;

    public Client(string address, int port) : base(address, port)
    {
    }

    public void DisconnectAndStop()
    {
        _stop = true;
        CloseAsync(1000);
        while (IsConnected)
            Thread.Yield();
    }

    public override void OnWsConnecting(HttpRequest request)
    {
        request.SetBegin("GET", "/");
        request.SetHeader("Host", "localhost");
        request.SetHeader("Origin", "http://localhost");
        request.SetHeader("Upgrade", "websocket");
        request.SetHeader("Connection", "Upgrade");
        request.SetHeader("Sec-WebSocket-Key", Convert.ToBase64String(WsNonce));
        request.SetHeader("Sec-WebSocket-Protocol", "chat, superchat");
        request.SetHeader("Sec-WebSocket-Version", "13");
        request.SetBody();
    }

    public override void OnWsConnected(HttpResponse response)
    {
        Bot.Logger.LogSuccess($"Chat WebSocket client connected a new session with Id {Id}");
    }

    public override void OnWsDisconnected()
    {
        Bot.Logger.LogError($"Chat WebSocket client disconnected a session with Id {Id}");
    }

    public override void OnWsReceived(byte[] buffer, long offset, long size)
    {
        Task.Run(async () =>
        {
            var jsonMessage = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            var message = JsonSerializer.Deserialize<Message>(jsonMessage);

            if (message.SenderType is SenderType.Discord)
                return;

            var guild = await Bot.Client.GetGuildAsync(1101889864523337848);
            var channel = guild.GetDefaultChannel();
            switch (message.MessageType)
            {
                case MessageType.Text:
                    Bot.Logger.LogInformation(
                        $"Received message from {message.SenderName}: " +
                        $"{message.User.FirstName} ({message.User.Id}) - {message.Content}");
                    await channel.SendMessageAsync($"{message.User.FirstName}: {message.Content}");
                    return;

                default:
                {
                    if (message.MessageType is MessageType.Album)
                        Bot.Logger.LogInformation(
                            $"Received album from {message.SenderName} with {message.MediaFiles.Count} files: " +
                            $"{message.User.FirstName} ({message.User.Id})");
                    else
                    {
                        
                    }
                    
                    var messageBuilder = new DiscordMessageBuilder();

                    foreach (var file in message.MediaFiles)
                    {
                        var memoryStream = new MemoryStream(file.Data);
                        messageBuilder.AddFile($"{file.Name}{file.Extension}", memoryStream);
                    }

                    messageBuilder.WithContent(
                        $"");
                    await channel.SendMessageAsync(messageBuilder);

                    return;
                }
            }
        });
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();

        Bot.Logger.LogError($"Chat WebSocket client disconnected a session with Id {Id}");

        Thread.Sleep(1000);

        if (!_stop)
            ConnectAsync();
    }

    protected override void OnError(SocketError error)
    {
        Bot.Logger.LogError($"Chat WebSocket client caught an error with code {error}");
    }
}