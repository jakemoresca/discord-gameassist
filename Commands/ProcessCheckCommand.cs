using System.Diagnostics;
using Discord;
using Discord.WebSocket;

public class ProcessCheckCommand : ISlashCommand
{
    public string CommandName => SlashCommands.ProcessCheck;

    public async Task Handle(SocketSlashCommand command)
    {
        var process = command.Data.Options.First().Value.ToString() ?? string.Empty;
        var processName = process.Split("|")[1];

        await command.FollowupAsync($"Ah eh... Checking the availability of {processName}");
        var isSuccess = CheckProcessIfRunning(processName, command);

        var embedBuilder = new EmbedBuilder()
            .WithAuthor(command.User)
            .WithTitle($"Availability of {processName}")
            .WithCurrentTimestamp();

        if (isSuccess)
        {
            embedBuilder.WithDescription($"{processName} is Online.")
                .WithColor(Color.Green);
        }
        else
        {
            embedBuilder.WithDescription($"{processName} is Offline.")
                .WithColor(Color.Red);
        }

        await command.FollowupAsync(embed: embedBuilder.Build());
    }

    bool CheckProcessIfRunning(string processName, SocketSlashCommand command)
    {
        if (processName.ToLower() == "cmd")
        {
            var processArgument = command.Data.Options.First().Value.ToString() ?? string.Empty;
            var title = processArgument.Split("|")[2];
            var processes = Process.GetProcessesByName(processName);

            foreach (var process in processes)
            {
                if (process.MainWindowTitle == title)
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            var processes = Process.GetProcessesByName(processName);

            return processes.Length > 0;
        }
    }
}
