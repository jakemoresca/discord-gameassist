public class SlashCommandService : ISlashCommandService
{
    private readonly ISlashCommandRepository _slashCommandRepository;

    public SlashCommandService(ISlashCommandRepository slashCommandRepository)
    {
        _slashCommandRepository = slashCommandRepository;
    }

    public ISlashCommand GetSlashCommand(string commandName)
    {
       var slashCommand = _slashCommandRepository.GetAll().GetValueOrDefault(commandName);
       return slashCommand;
    }
}
