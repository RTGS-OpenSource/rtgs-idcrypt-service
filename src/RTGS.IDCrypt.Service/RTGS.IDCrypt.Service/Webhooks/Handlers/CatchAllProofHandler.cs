namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class CatchAllProofHandler : IMessageHandler
{
	private readonly ILogger<CatchAllProofHandler> _logger;

	public CatchAllProofHandler(ILogger<CatchAllProofHandler> logger)
	{
		_logger = logger;
	}
	public string MessageType => "present_proof";

	public Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		_logger.LogInformation(jsonMessage);

		return Task.CompletedTask;
	}
}
