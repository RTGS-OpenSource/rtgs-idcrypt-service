namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public interface IBasicMessageHandler
{
	public string MessageType { get; }

	Task HandleAsync(string message, string connectionId, CancellationToken cancellationToken = default);
}
