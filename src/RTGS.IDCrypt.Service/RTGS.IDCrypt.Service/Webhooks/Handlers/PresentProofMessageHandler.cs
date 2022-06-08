using System.Text.Json;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.BasicMessage;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class PresentProofMessageHandler : IMessageHandler
{
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly IRtgsConnectionRepository _rtgsConnectionRepository;
	private readonly IBasicMessageClient _basicMessageClient;
	private readonly CoreConfig _coreConfig;
	private readonly ILogger<PresentProofMessageHandler> _logger;

	public PresentProofMessageHandler(
		IBankPartnerConnectionRepository bankPartnerConnectionRepository,
		IRtgsConnectionRepository rtgsConnectionRepository,
		IBasicMessageClient basicMessageClient,
		IOptions<CoreConfig> coreConfigOptions,
		ILogger<PresentProofMessageHandler> logger = null)
	{
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
		_rtgsConnectionRepository = rtgsConnectionRepository;
		_basicMessageClient = basicMessageClient;
		_logger = logger;
		_coreConfig = coreConfigOptions.Value;
	}

	public string MessageType => "present_proof";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		_logger?.LogInformation("Present proof webhook called with payload: {Payload}", jsonMessage);

		var proof = JsonSerializer.Deserialize<Proof>(jsonMessage);

		if (proof!.State != ProofStates.Received)
		{
			return;
		}

		await _bankPartnerConnectionRepository.ActivateAsync(proof!.ConnectionId, cancellationToken);

		var bankConnection = await _bankPartnerConnectionRepository.GetAsync(proof.ConnectionId, cancellationToken);

		if (bankConnection.Role == ConnectionRoles.Invitee)
		{
			var rtgsConnection = await _rtgsConnectionRepository.GetEstablishedAsync(cancellationToken);

			var setBankPartnershipOnlineRequest = new SetBankPartnershipOnlineRequest
			{
				ApprovingBankDid = _coreConfig.RtgsGlobalId,
				RequestingBankDid = bankConnection.PartitionKey //TODO - get from proof
			};

			await _basicMessageClient.SendAsync(rtgsConnection.ConnectionId, "set-bank-partnership-online", setBankPartnershipOnlineRequest, cancellationToken);
		}
	}
}
