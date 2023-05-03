using Discord.WebSocket;

public class SlashCommandHandler : ISlashCommandHandler
{
    private readonly ISlashCommandService _slashCommandService;

    public SlashCommandHandler(ISlashCommandService slashCommandService)
    {
        _slashCommandService = slashCommandService;
    }

    public async Task Handle(SocketSlashCommand command)
    {
        await HandleJoelFlavoringFollowUps(command);

        var commandName = command.Data.Name;
        var slashCommand = _slashCommandService.GetSlashCommand(commandName);
        await slashCommand.Handle(command);
    }

    private async Task HandleJoelFlavoringFollowUps(SocketSlashCommand command)
    {
        await command.RespondAsync("Ah. Eh..");
        await Task.Delay(3000);
    }
}
