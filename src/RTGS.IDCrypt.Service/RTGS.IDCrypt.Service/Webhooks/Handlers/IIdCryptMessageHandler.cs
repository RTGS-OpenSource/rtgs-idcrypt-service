namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public interface IIdCryptMessageHandler
{
	string MessageType { get; }
	Task HandleAsync(string jsonMessage);
}
