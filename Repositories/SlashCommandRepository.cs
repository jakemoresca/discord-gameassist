public class SlashCommandRepository : ISlashCommandRepository
{
    private readonly IEnumerable<ISlashCommand> _slashCommands;

    public SlashCommandRepository(IEnumerable<ISlashCommand> slashCommands)
    {
        _slashCommands = slashCommands;
    }

    public IReadOnlyDictionary<string, ISlashCommand> GetAll()
    {
        var slashCommandDictionary = _slashCommands.ToDictionary(x => x.CommandName);
        return slashCommandDictionary;
    }
}
