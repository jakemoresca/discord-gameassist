using System.Net.NetworkInformation;
using System.Text;
using Discord;
using Discord.WebSocket;

public class PingCommand : ISlashCommand
{
    public string CommandName => SlashCommands.Ping;

    public async Task Handle(SocketSlashCommand command)
    {
        var host = command.Data.Options.First().Value.ToString() ?? string.Empty;
        await command.FollowupAsync($"Checking the availability of {host}");

        var hasPort = host.Contains(":");
        var isSuccess = PingHostWithoutPort(host);

        var embedBuilder = new EmbedBuilder()
            .WithAuthor(command.User)
            .WithTitle($"Availability of {host}")
            .WithCurrentTimestamp();

        if (isSuccess)
        {
            embedBuilder.WithDescription($"{host} is Online.")
                .WithColor(Color.Green);
        }
        else
        {
            embedBuilder.WithDescription($"{host} is Offline.")
                .WithColor(Color.Red);
        }

        await command.FollowupAsync(embed: embedBuilder.Build());
    }

    static bool PingHostWithoutPort(string hostUri)
    {
        Ping pingSender = new Ping();
        string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        byte[] buffer = Encoding.ASCII.GetBytes(data);
        PingOptions options = new PingOptions(64, true);
        AutoResetEvent waiter = new AutoResetEvent(false);

        PingReply reply = pingSender.Send(hostUri, 12000, buffer, options);

        return reply.Status == IPStatus.Success;
    }
}
