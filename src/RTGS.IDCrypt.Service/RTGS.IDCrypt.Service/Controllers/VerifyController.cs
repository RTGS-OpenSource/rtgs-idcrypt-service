using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VerifyController : ControllerBase
{
	private readonly ILogger<VerifyController> _logger;
	private readonly BankPartnerConnectionsConfig _bankPartnerConnectionsConfig;
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly IJsonSignaturesClient _jsonSignaturesClient;
	private readonly IWalletClient _walletClient;

	public VerifyController(
		ILogger<VerifyController> logger,
		IOptions<BankPartnerConnectionsConfig> bankPartnerConnectionsConfig,
		IStorageTableResolver storageTableResolver,
		IJsonSignaturesClient jsonSignaturesClient,
		IWalletClient walletClient)
	{
		_logger = logger;
		_bankPartnerConnectionsConfig = bankPartnerConnectionsConfig.Value;
		_storageTableResolver = storageTableResolver;
		_jsonSignaturesClient = jsonSignaturesClient;
		_walletClient = walletClient;
	}

	[HttpPost]
	public async Task<IActionResult> Post(
		VerifyPrivateSignatureRequest verifyPrivateSignatureRequest,
		CancellationToken cancellationToken = default)
	{
		var bankPartnerConnectionsTable = _storageTableResolver.GetTable(
			_bankPartnerConnectionsConfig.BankPartnerConnectionsTableName);

		var bankPartnerConnections = bankPartnerConnectionsTable
			.Query<BankPartnerConnection>(cancellationToken: cancellationToken)
			.Where(bankPartnerConnection =>
				bankPartnerConnection.PartitionKey == verifyPrivateSignatureRequest.RtgsGlobalId
				&& bankPartnerConnection.RowKey == verifyPrivateSignatureRequest.Alias)
			.ToList();

		if (!bankPartnerConnections.Any())
		{
			_logger.LogError(
				"No bank partner connection found for RTGS Global ID {RtgsGlobalId} and Alias {Alias}",
				verifyPrivateSignatureRequest.RtgsGlobalId,
				verifyPrivateSignatureRequest.Alias);

			return NotFound();
		}

		var bankPartnerConnection = bankPartnerConnections.Single();

		bool verified;
		try
		{
			verified = await _jsonSignaturesClient.VerifyJsonDocumentPrivateSignatureAsync(
				verifyPrivateSignatureRequest.Message,
				verifyPrivateSignatureRequest.PrivateSignature,
				bankPartnerConnection.ConnectionId,
				cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when sending VerifyPrivateSignature request to ID Crypt Cloud Agent");

			throw;
		}

		var verifyPrivateSignatureResponse = new VerifyPrivateSignatureResponse
		{
			Verified = verified
		};

		return Ok(verifyPrivateSignatureResponse);
	}

	public async Task<IActionResult> VerifyPublicSignature(VerifyPublicSignatureRequest verifyPublicSignatureRequest, CancellationToken cancellationToken)
	{
		string publicDid;

		try
		{
			publicDid = await _walletClient.GetPublicDidAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent");

			throw;
		}

		bool verified;
		try
		{
			verified = await _jsonSignaturesClient.VerifyJsonDocumentPublicSignatureAsync(
				verifyPublicSignatureRequest.Message,
				verifyPublicSignatureRequest.PublicSignature, publicDid, cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when sending VerifyPublicSignature request to ID Crypt Cloud Agent");

			throw;
		}


		return Ok(new VerifyPublicSignatureResponse { Verified = verified });
	}
}
