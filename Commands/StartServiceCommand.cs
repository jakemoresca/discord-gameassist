using System.Diagnostics;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

public class StartServiceCommand : ISlashCommand
{
    private readonly IConfigurationRoot _config;

    public StartServiceCommand(IConfigurationRoot config)
    {
        _config = config;
    }

    public string CommandName => SlashCommands.StartService;

    public async Task Handle(SocketSlashCommand command)
    {
        var serviceName = command.Data.Options.First().Value.ToString() ?? string.Empty;
        await command.FollowupAsync($"Starting {serviceName}");

        try
        {
            var appsFolder =  _config.GetRequiredSection("Settings")["AppsFolder"] ?? string.Empty;
            var applicationFolderPath = Path.Combine(appsFolder, serviceName);

            string[] extensions = new[] { ".exe", ".bat", ".cmd", ".lnk" };

            FileInfo[] files = new DirectoryInfo(applicationFolderPath)
                .EnumerateFiles()
                     .Where(f => extensions.Contains(f.Extension.ToLower()))
                     .ToArray();

            var process = new ProcessStartInfo(files[0].FullName);
            process.WorkingDirectory = applicationFolderPath;
            process.UseShellExecute = true;
            Process.Start(process);
        }
        catch (Exception ex)
        {
            await command.FollowupAsync($"Failed to start {serviceName}. {ex.Message}");
        }

        await command.FollowupAsync($"Started {serviceName} successfully.");
    }
}
