﻿using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DSharpPlus.Entities;
using MSyncBot.Discord.Handlers.Server.Types;
using MSyncBot.Discord.Handlers.Server.Types.Enums;
using NetCoreServer;

namespace MSyncBot.Discord.Handlers.Server;

public class Client : WsClient
{
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
                    Bot.Logger.LogInformation($"Received message: {message.SenderName} - {message.Content}");
                    await channel.SendMessageAsync($"{message.Content}\n\n" +
                        $"Время, за которое сообщение пришло: {DateTime.Now - message.Timestamp}");
                    return;

                case MessageType.Photo:
                {
                    Bot.Logger.LogInformation($"Received photo: {message.SenderName} - " +
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");

                    var userPhotosPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserPhotos");
                    Directory.CreateDirectory(userPhotosPath);
                    var filePath = Path.Combine(userPhotosPath,
                        $"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}");
                    await File.WriteAllBytesAsync(filePath, message.MediaFiles[0].Data);

                    await using Stream stream = File.OpenRead(filePath);
                    var messageBuilder = new DiscordMessageBuilder()
                        .WithContent($"Время, за которое сообщение пришло: {DateTime.Now - message.Timestamp}")
                        .AddFile($"{message.MediaFiles[0].Name}{message.MediaFiles[0].Extension}", stream);
                    await channel.SendMessageAsync(messageBuilder);
                    File.Delete(filePath);
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

    private bool _stop;
}