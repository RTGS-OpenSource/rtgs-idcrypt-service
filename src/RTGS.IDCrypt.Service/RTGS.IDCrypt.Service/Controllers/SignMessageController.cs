using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignMessageController : ControllerBase
{
	private readonly ILogger<SignMessageController> _logger;
	private readonly BankPartnerConnectionsConfig _bankPartnerConnectionsConfig;
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly IJsonSignaturesClient _jsonSignaturesClient;

	public SignMessageController(
		ILogger<SignMessageController> logger,
		IOptions<BankPartnerConnectionsConfig> bankPartnerConnectionsConfig,
		IStorageTableResolver storageTableResolver,
		IJsonSignaturesClient jsonSignaturesClient)
	{
		_logger = logger;
		_bankPartnerConnectionsConfig = bankPartnerConnectionsConfig.Value;
		_storageTableResolver = storageTableResolver;
		_jsonSignaturesClient = jsonSignaturesClient;
	}

	[HttpPost]
	public async Task<IActionResult> Post(
		SignMessageRequest signMessageRequest,
		CancellationToken cancellationToken)
	{
		var bankPartnerConnectionsTable = _storageTableResolver.GetTable(_bankPartnerConnectionsConfig.BankPartnerConnectionsTableName);

		var bankPartnerConnections = bankPartnerConnectionsTable
			.Query<BankPartnerConnection>(cancellationToken: cancellationToken)
			.Where(bankPartnerConnection =>
				bankPartnerConnection.PartitionKey == signMessageRequest.RtgsGlobalId)
			.ToList();

		if (!bankPartnerConnections.Any())
		{
			_logger.LogError(
				"No bank partner connection found for RTGS Global ID {RtgsGlobalId}",
				signMessageRequest.RtgsGlobalId);

			return NotFound();
		}

		var bankPartnerConnection = bankPartnerConnections.First();

		SignDocumentResponse signDocumentResponse;

		try
		{
			signDocumentResponse = await _jsonSignaturesClient.SignJsonDocumentAsync(
				signMessageRequest.Message,
				bankPartnerConnection.ConnectionId,
				cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when signing JSON document");

			throw;
		}

		var signMessageResponse = new SignMessageResponse
		{
			PairwiseDidSignature = signDocumentResponse.PairwiseDidSignature,
			PublicDidSignature = signDocumentResponse.PublicDidSignature,
			Alias = bankPartnerConnection.RowKey
		};

		return Ok(signMessageResponse);
	}
}
