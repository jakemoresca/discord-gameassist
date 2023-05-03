using System.Diagnostics;
using Discord.WebSocket;

public class StopServiceCommand : ISlashCommand
{
    public string CommandName => SlashCommands.StopService;

    public async Task Handle(SocketSlashCommand command)
    {
        var serviceName = command.Data.Options.First().Value.ToString() ?? string.Empty;
        await command.FollowupAsync($"Stopping {serviceName}");

        try
        {
            Process[] processes = Process.GetProcessesByName(serviceName);
            foreach (Process process in processes)
            {
                process.Kill();
            }
        }
        catch (Exception ex)
        {
            await command.FollowupAsync($"Failed to stop {serviceName}. {ex.Message}");
        }

        await command.FollowupAsync($"Stopped {serviceName} successfully.");
    }
}
