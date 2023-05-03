// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using DiscordGameAssist.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

var config = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var discordConfig = new DiscordSocketConfig
{
    LogLevel = LogSeverity.Debug
};

var client = new DiscordSocketClient(discordConfig);

var commands = new CommandService(new CommandServiceConfig
{
    LogLevel = LogSeverity.Info,
    CaseSensitiveCommands = false,
});

// Setup your DI container.
Bootstrapper.Init();
Bootstrapper.RegisterInstance(client);
Bootstrapper.RegisterInstance(commands);
Bootstrapper.RegisterInstance(config);

RegisterSlashCommands();
RegisterServices();
RegisterRepositories();
RegisterFactories();

void RegisterRepositories()
{
    Bootstrapper.RegisterType<ISlashCommandRepository, SlashCommandRepository>();
}

void RegisterFactories()
{
    Bootstrapper.RegisterType<ISlashCommandFactory, SlashCommandFactory>();
}

void RegisterServices()
{
    Bootstrapper.RegisterType<ISlashCommandService, SlashCommandService>();
    Bootstrapper.RegisterType<ICommandHandler, CommandHandler>();
    Bootstrapper.RegisterType<ISlashCommandHandler, SlashCommandHandler>();
}

void RegisterSlashCommands()
{
    Bootstrapper.RegisterType<ISlashCommand, GenerateMemeCommand>();
    Bootstrapper.RegisterType<ISlashCommand, JokeCommand>();
    Bootstrapper.RegisterType<ISlashCommand, PingCommand>();
    Bootstrapper.RegisterType<ISlashCommand, PlexInviteCommand>();
    Bootstrapper.RegisterType<ISlashCommand, ProcessCheckCommand>();
    Bootstrapper.RegisterType<ISlashCommand, StartServiceCommand>();
}

await MainAsync();

async Task MainAsync()
{
    client.Ready += Client_Ready;

    var token = config.GetRequiredSection("Settings")["DiscordBotToken"];
    if (string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine("Token is null or empty.");
        return;
    }

    await client.LoginAsync(TokenType.Bot, token);
    await client.StartAsync();

    // Wait infinitely so your bot actually stays connected.
    await Task.Delay(Timeout.Infinite);
}

async Task Client_Ready()
{
    Console.WriteLine("Client Ready");

    var slashCommandFactory = Bootstrapper.ServiceProvider.GetRequiredService<ISlashCommandFactory>();
    var slashCommandBuilders = slashCommandFactory.CreateSlashCommandBuilders();

    var slashCommandHandler = Bootstrapper.ServiceProvider.GetRequiredService<ISlashCommandHandler>();
    client.SlashCommandExecuted += slashCommandHandler.Handle;

    try
    {
        foreach (var slashCommandBuilder in slashCommandBuilders)
        {
            await client.CreateGlobalApplicationCommandAsync(slashCommandBuilder.Build());
        }
    }
    catch (HttpException exception)
    {
        var json = JsonConvert.SerializeObject(exception.Message, Formatting.Indented);
        Console.WriteLine(json);
    }
}