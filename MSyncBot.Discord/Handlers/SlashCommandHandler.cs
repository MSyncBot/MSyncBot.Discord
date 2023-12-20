using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;

namespace MSyncBot.Discord.Handlers;

public class SlashCommandHandler
{
    public static Task ExecuteHandlerAsync(SlashCommandsExtension sender, SlashCommandExecutedEventArgs args)
    {
        Bot.Logger.LogSuccess(args.Context.User.Username + " have been used " +
                              args.Context.Interaction.Data.Name);
        return Task.CompletedTask;
    }

    public static Task InvokeHandlerAsync(SlashCommandsExtension sender, SlashCommandInvokedEventArgs args)
    {
        Bot.Logger.LogProcess(args.Context.User.Username + " using command " +
                              args.Context.Interaction.Data.Name);
        return Task.CompletedTask;
    }

    public static Task ErrorHandlerAsync(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
    {
        Bot.Logger.LogError(args.Exception.Message);
        return Task.CompletedTask;
    }
}