using Discord.WebSocket;

public interface ISlashCommandHandler
{
    Task Handle(SocketSlashCommand command);
}