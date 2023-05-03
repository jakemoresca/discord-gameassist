public interface ISlashCommandRepository 
{
    public IReadOnlyDictionary<string, ISlashCommand> GetAll();
}