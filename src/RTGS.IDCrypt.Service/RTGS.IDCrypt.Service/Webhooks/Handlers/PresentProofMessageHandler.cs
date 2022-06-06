using System.Text.Json;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Webhooks.Models;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.BasicMessage.Models;

namespace RTGS.IDCrypt.Service.Webhooks.Handlers;

public class PresentProofMessageHandler : IMessageHandler
{
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly IRtgsConnectionRepository _rtgsConnectionRepository;
	private readonly IBasicMessageClient _basicMessageClient;
	private readonly CoreConfig _coreConfig;

	public PresentProofMessageHandler(
		IBankPartnerConnectionRepository bankPartnerConnectionRepository,
		IRtgsConnectionRepository rtgsConnectionRepository,
		IBasicMessageClient basicMessageClient,
		IOptions<CoreConfig> coreConfigOptions)
	{
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
		_rtgsConnectionRepository = rtgsConnectionRepository;
		_basicMessageClient = basicMessageClient;
		_coreConfig = coreConfigOptions.Value;
	}

	public string MessageType => "present_proof";

	public async Task HandleAsync(string jsonMessage, CancellationToken cancellationToken)
	{
		var proof = JsonSerializer.Deserialize<Proof>(jsonMessage);

		await _bankPartnerConnectionRepository.ActivateAsync(proof.ConnectionId, cancellationToken);

		var bankConnection = await _bankPartnerConnectionRepository.GetAsync(proof.ConnectionId, cancellationToken);

		if (bankConnection.Role is ConnectionRoles.Invitee)
		{
			var rtgsConnection = await _rtgsConnectionRepository.GetEstablishedAsync(cancellationToken);

			var setBankPartnershipOnlineRequest = new SetBankPartnershipOnlineRequest
			{
				ApprovingBankDid = _coreConfig.RtgsGlobalId,
				RequestingBankDid = "requesting-bank-rtgs-global-id" //TODO - get from proof
			};

			await _basicMessageClient.SendAsync(rtgsConnection.ConnectionId, "set-bank-partnership-online", setBankPartnershipOnlineRequest, cancellationToken);
		}
	}
}
