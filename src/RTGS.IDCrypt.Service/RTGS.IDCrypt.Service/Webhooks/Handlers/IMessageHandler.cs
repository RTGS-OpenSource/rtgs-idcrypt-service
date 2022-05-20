namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public interface IMessageHandler
{
	string MessageType { get; }
	Task HandleAsync(string jsonMessage, CancellationToken cancellationToken);
}
