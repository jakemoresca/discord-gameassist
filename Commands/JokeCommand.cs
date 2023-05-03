using Discord;
using Discord.WebSocket;

public class JokeCommand : ISlashCommand
{
    public string CommandName => SlashCommands.Joke;

    public async Task Handle(SocketSlashCommand command)
    {
        var category = command.Data.Options.First().Value.ToString() ?? "Any";

        using var client = new HttpClient();
        var response = await client.GetAsync($"https://v2.jokeapi.dev/joke/{category}?format=txt");

        var embedBuilder = new EmbedBuilder()
        .WithAuthor(command.User)
        .WithTitle($"Joel's joke for the day")
        .WithCurrentTimestamp();

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            embedBuilder.WithDescription(responseContent)
                .WithColor(Color.Green);
        }
        else
        {
            embedBuilder.WithDescription($"Catastrophic! I cannot present.")
                .WithColor(Color.Red);
        }

        await command.FollowupAsync(embed: embedBuilder.Build());
    }
}