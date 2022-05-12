namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public interface IIdCryptMessageHandler
{
	string MessageType { get; }
	void Handle(string jsonMessage);
}
