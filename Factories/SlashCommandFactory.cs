using Discord;
using Microsoft.Extensions.Configuration;

public class SlashCommandFactory : ISlashCommandFactory
{
    private readonly IConfigurationRoot _config;

    public SlashCommandFactory(IConfigurationRoot config)
    {
        _config = config;
    }

    public IEnumerable<SlashCommandBuilder> CreateSlashCommandBuilders()
    {
        var pingCommand = new SlashCommandBuilder();
        pingCommand.WithName(SlashCommands.ServiceCheck)
            .WithDescription("Check if the service is available.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("server")
                .WithDescription("Select the server you want to check.")
                .WithRequired(true)
                .AddChoicesFromOptions(SlashCommands.ServiceCheck, _config)
                .WithType(ApplicationCommandOptionType.String)
            );

        var startServiceCommand = new SlashCommandBuilder();
        startServiceCommand.WithName(SlashCommands.StartService)
            .WithDescription("Start a service if not already started.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("server")
                .WithDescription("Select the server you want to start.")
                .WithRequired(true)
                .AddChoicesFromOptions(SlashCommands.StartService, _config)
                .WithType(ApplicationCommandOptionType.String)
            );

        var stopServiceCommand = new SlashCommandBuilder();
        stopServiceCommand.WithName(SlashCommands.StopService)
            .WithDescription("Stop a service.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("server")
                .WithDescription("Select the server you want to stop.")
                .WithRequired(true)
                .AddChoicesFromOptions(SlashCommands.StopService, _config)
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
                .AddChoicesFromOptions(SlashCommands.Joke, _config)
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
            plexInviteCommand,
            stopServiceCommand
        };

        return builders;
    }
}

static class SlashCommandBuilderExtensions
{
    public static SlashCommandOptionBuilder AddChoicesFromOptions(this SlashCommandOptionBuilder builder, string serviceName, IConfigurationRoot config)
    {
        var commandChoices =  config.GetRequiredSection("Commands")
            .GetRequiredSection(serviceName)
            .GetRequiredSection("choices")
            .GetChildren();

        foreach (var commandChoice in commandChoices)
        {
            var name = commandChoice["name"];
            var value = commandChoice["value"];
            builder.AddChoice(name, value);   
        }

        return builder;
    }
}