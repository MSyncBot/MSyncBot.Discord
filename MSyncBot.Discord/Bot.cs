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
    public Bot(string token, string serverIp, int serverPort, MLogger logger)
    {
        Logger = logger;
        Logger.LogProcess("Initializing the bot...");

        Token = token;
        Server = new ServerHandler(serverIp, serverPort);

        Logger.LogSuccess("The bot has been successfully initialized.");
    }

    private string? Token { get; }
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

        Client.Ready += ReadyHandler.GetReady;
        Client.ClientErrored += ErrorHandler.GetError;
        //Client.GuildAvailable += Guild.GuildAvailableHandler;
        //Client.GuildCreated += Guild.GuildCreatedHandlerAsync;
        Client.MessageCreated += MessageHandler.MessageCreated;

        var slashCommands = Client.UseSlashCommands();
        slashCommands.SlashCommandErrored += SlashCommandHandler.ErrorHandlerAsync;
        slashCommands.SlashCommandInvoked += SlashCommandHandler.InvokeHandlerAsync;
        slashCommands.SlashCommandExecuted += SlashCommandHandler.ExecuteHandlerAsync;

        var activity = new DiscordActivity("Sync messages", ActivityType.Playing);

        await Client.ConnectAsync(activity, UserStatus.Online);

        Server.ConnectAsync();

        Logger.LogSuccess("The bot is running.");
    }


    public async Task StopAsync()
    {
        await Client.DisconnectAsync();
    }
}