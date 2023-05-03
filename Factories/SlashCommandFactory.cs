using Discord;

public class SlashCommandFactory : ISlashCommandFactory
{
    public IEnumerable<SlashCommandBuilder> CreateSlashCommandBuilders()
    {
        var pingCommand = new SlashCommandBuilder();
        pingCommand.WithName(SlashCommands.ServiceCheck)
            .WithDescription("Check if the service is available.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("server")
                .WithDescription("Select the server you want to check.")
                .WithRequired(true)
                .AddChoice("Mediaserver", "mediaserver")
                .AddChoice("Project Zomboid", "process|cmd|StartServer64_nosteam.bat - Shortcut")
                .AddChoice("The Forest", "process|TheForestDedicatedServer")
                .WithType(ApplicationCommandOptionType.String)
            );

        var startServiceCommand = new SlashCommandBuilder();
        startServiceCommand.WithName(SlashCommands.StartService)
            .WithDescription("Start a service if not already started.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("server")
                .WithDescription("Select the server you want to check.")
                .WithRequired(true)
                .AddChoice("Project Zomboid", "project-zomboid")
                .AddChoice("The Forest", "the-forest-link")
                .WithType(ApplicationCommandOptionType.String)
            );

        var plexInviteCommand = new SlashCommandBuilder();
        plexInviteCommand.WithName(SlashCommands.PlexInvite)
            .WithDescription("Join PressX Media Plex Server.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("email")
                .WithDescription("Type in the email you use to register at plex.tv")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            );

        var jokeCommand = new SlashCommandBuilder();
        jokeCommand.WithName(SlashCommands.Joke)
            .WithDescription("Get some inspirational thoughts from our idol.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("category")
                .WithDescription("Select the category of the joke you want.")
                .WithRequired(true)
                .AddChoice("Any", "Any")
                .AddChoice("Programming", "Programming")
                .AddChoice("Misc", "Misc")
                .AddChoice("Dark", "Dark")
                .AddChoice("Pun", "Pun")
                .AddChoice("Spooky", "Spooky")
                .AddChoice("Christmas", "Christmas")
                .WithType(ApplicationCommandOptionType.String)
            );

        var generateMemeCommand = new SlashCommandBuilder();
        generateMemeCommand.WithName(SlashCommands.Joke)
            .WithDescription("The JoelGPT we are waiting for! Generate some meme.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("prompt")
                .WithDescription("Type in the meme you want to generate.")
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            );

        var builders = new List<SlashCommandBuilder>
        {
            generateMemeCommand,
            jokeCommand,
            pingCommand,
            plexInviteCommand
        };

        return builders;
    }
}