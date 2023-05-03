using Discord.WebSocket;

public class ServiceCheckCommand : ISlashCommand
{
    private readonly ISlashCommandService _slashCommandService;
    private readonly ISlashCommand _processCheckCommand;
    private readonly ISlashCommand _pingCommand;
    private const string PROCESS_NAME_PREFIX = "process";

    public string CommandName => SlashCommands.ServiceCheck;

    public ServiceCheckCommand(ISlashCommandService slashCommandService)
    {
        _slashCommandService = slashCommandService;
        _processCheckCommand = _slashCommandService.GetSlashCommand(SlashCommands.ProcessCheck);
        _pingCommand = _slashCommandService.GetSlashCommand(SlashCommands.Ping);
    }

    public async Task Handle(SocketSlashCommand command)
    {
        var serviceName = command.Data.Options.First().Value.ToString() ?? string.Empty;
        var isProcessCheck = serviceName.StartsWith(PROCESS_NAME_PREFIX);

        if (isProcessCheck)
        {
            await _processCheckCommand.Handle(command);
        }
        else
        {
            await _pingCommand.Handle(command);
        }
    }
}
