using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

public class PlexInviteCommand : ISlashCommand
{
    private readonly IConfigurationRoot _config;

    public PlexInviteCommand(IConfigurationRoot config)
    {
        _config = config;
    }

    public string CommandName => SlashCommands.PlexInvite;

    public async Task Handle(SocketSlashCommand command)
    {
        var inviteeEmail = command.Data.Options.First().Value.ToString() ?? string.Empty;

        await command.FollowupAsync($"Creating invitation...");

        // Create an HTTP client
        var client = new HttpClient();

        // Set the Authorization header to include the user's Plex token
        var baseUrl = "https://plex.tv";
        var serverId = _config.GetRequiredSection("Settings")["ServerId"];
        var plexToken = _config.GetRequiredSection("Settings")["PlexToken"];
        client.DefaultRequestHeaders.Add("X-Plex-Token", plexToken);
        client.DefaultRequestHeaders.Add("X-Plex-Client-Identifier", "au9tfexlaznwm2n5cckacppg");

        // Build the URL for the invite endpoint
        string url = $"{baseUrl}/api/v2/shared_servers";

        // Create the JSON body for the request
        var json = new JObject
        {
            { "invitedEmail", inviteeEmail },
            { "settings",
                new JObject{
                    { "allowChannels", true },
                    { "allowSubtitleAdmin", true },
                    { "allowSync", true },
                    { "allowTuners", 0 },
                    {"filterMovies", ""},
                    {"filterMusic", ""},
                    {"filterTelevision", ""}
                }
            },
            { "librarySectionIds", new JArray {112155685, 112155692} },
            { "machineIdentifier", "2a557d987b9d87b6ec2776d7acbfb2cac5fff185" }
        };

        // Make the request to the Plex API
        var response = client.PostAsync(url, new StringContent(json.ToString(), System.Text.Encoding.UTF8, "application/json")).Result;

        // check if request is successfull
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Invitation sent successfully to: {inviteeEmail}");
            await command.FollowupAsync($"Invitation sent successfully to: {inviteeEmail}");
        }
        else
        {
            Console.WriteLine($"Failed to send the invitation, please check the credentials and the serverId. reason: {await response.Content.ReadAsStringAsync()}");
            await command.FollowupAsync("Failed to send the invitation, please check the credentials and the serverId");
        }
    }
}
