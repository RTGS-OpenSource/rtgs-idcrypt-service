namespace RTGS.IDCrypt.Service.Webhooks.Handlers.BasicMessage;

public interface IBasicMessageHandler
{
	public string ForType { get; }

	Task HandleAsync(string message, CancellationToken cancellationToken = default);
}
