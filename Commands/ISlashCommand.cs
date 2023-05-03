using Discord.WebSocket;

public interface ISlashCommand
{
    string CommandName { get; }
    Task Handle(SocketSlashCommand command);
}