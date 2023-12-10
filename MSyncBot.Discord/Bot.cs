using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using MLoggerService;
using MSyncBot.Discord.Handlers;
using MSyncBot.Discord.Handlers.Server;

namespace MSyncBot.Discord;

public class Bot
{
    public Bot(string token, MLogger logger, MDatabase.MDatabase database)
    {
        Logger = logger;
        Logger.LogProcess("Initializing the bot...");

        Token = token;
        Database = database;
        Server = new ServerHandler("127.0.0.1");
        
        Logger.LogSuccess("The bot has been successfully initialized.");
    }

    private string Token { get; }
    public static MLogger Logger { get; private set; }
    public static MDatabase.MDatabase Database { get; private set; }
    public static DiscordClient Client { get; private set; }
    public static ServerHandler Server { get; set; }

    public async Task StartAsync()
    {
        Logger.LogProcess("Starting the bot...");

        var botConfig = new DiscordConfiguration
        {
            Token = Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.Guilds
                | DiscordIntents.GuildVoiceStates
                | DiscordIntents.GuildIntegrations
                | DiscordIntents.GuildMessages
                | DiscordIntents.MessageContents,
            AutoReconnect = true,
            MinimumLogLevel = LogLevel.None
        };

        Client = new DiscordClient(botConfig);

        Client.Ready += Ready.ReadyHandler;
        Client.ClientErrored += Error.ErrorHandler;
        //Client.GuildAvailable += Guild.GuildAvailableHandler;
        //Client.GuildCreated += Guild.GuildCreatedHandlerAsync;
        Client.MessageCreated += Message.MessageCreatedHandler;
        
        var slashCommands = Client.UseSlashCommands();
        slashCommands.SlashCommandErrored += SlashCommand.ErrorHandlerAsync;
        slashCommands.SlashCommandInvoked += SlashCommand.InvokeHandlerAsync;
        slashCommands.SlashCommandExecuted += SlashCommand.ExecuteHandlerAsync;

        var activity = new DiscordActivity("Sync messages", ActivityType.Playing);

        await Client.ConnectAsync(activity, UserStatus.Online);

        _ = Task.Run(async () =>
        {
            await Server.ConnectToServerAsync();
            var stream = Server.TcpClient.GetStream();
            while (Server.TcpClient.Connected)
            {
                var client = await Server.ReceiveMessageAsync(stream);
                Logger.LogInformation(client.Name + ": " + client.Message);
                var guild = await Client.GetGuildAsync(1101889864523337848);
                await guild.GetDefaultChannel().SendMessageAsync(client.Name + ": " + client.Message);
            }
        });
        
        Logger.LogSuccess("The bot is running.");
    }
    

    public async Task StopAsync() => await Client.DisconnectAsync();
}