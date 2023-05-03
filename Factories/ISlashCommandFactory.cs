using Discord;

public interface ISlashCommandFactory
{
    IEnumerable<SlashCommandBuilder> CreateSlashCommandBuilders(); 
}