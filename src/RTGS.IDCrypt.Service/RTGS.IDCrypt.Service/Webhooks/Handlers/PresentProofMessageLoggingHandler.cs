namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class PresentProofMessageLoggingHandler : IMessageHandler
{
	private readonly ILogger<PresentProofMessageLoggingHandler> _logger;

	public PresentProofMessageLoggingHandler(ILogger<PresentProofMessageLoggingHandler> logger)
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
