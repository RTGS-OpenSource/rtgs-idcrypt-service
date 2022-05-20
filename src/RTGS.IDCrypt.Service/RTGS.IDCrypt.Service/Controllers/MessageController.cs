using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
	private readonly ILogger<MessageController> _logger;
	private readonly BankPartnerConnectionsConfig _bankPartnerConnectionsConfig;
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly IJsonSignaturesClient _jsonSignaturesClient;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IWalletClient _walletClient;

	public MessageController(
		ILogger<MessageController> logger,
		IOptions<BankPartnerConnectionsConfig> bankPartnerConnectionsConfig,
		IStorageTableResolver storageTableResolver,
		IJsonSignaturesClient jsonSignaturesClient,
		IDateTimeProvider dateTimeProvider,
		IWalletClient walletClient)
	{
		_logger = logger;
		_bankPartnerConnectionsConfig = bankPartnerConnectionsConfig.Value;
		_storageTableResolver = storageTableResolver;
		_jsonSignaturesClient = jsonSignaturesClient;
		_dateTimeProvider = dateTimeProvider;
		_walletClient = walletClient;
	}

	/// <summary>
	/// Endpoint to sign a document.
	/// </summary>
	/// <param name="signMessageRequest">The data required to sign a message.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="SignDocumentResponse"/></returns>
	[HttpPost("sign")]
	public async Task<IActionResult> Sign(
		SignMessageRequest signMessageRequest,
		CancellationToken cancellationToken)
	{
		var bankPartnerConnectionsTable = _storageTableResolver.GetTable(_bankPartnerConnectionsConfig.BankPartnerConnectionsTableName);

		var dateThreshold = _dateTimeProvider.UtcNow.Subtract(_bankPartnerConnectionsConfig.MinimumConnectionAge);

		var bankPartnerConnections = bankPartnerConnectionsTable
			.Query<BankPartnerConnection>(cancellationToken: cancellationToken)
			.Where(bankPartnerConnection =>
				bankPartnerConnection.PartitionKey == signMessageRequest.RtgsGlobalId
				&& bankPartnerConnection.CreatedAt <= dateThreshold).ToList();

		var bankPartnerConnection = bankPartnerConnections.MaxBy(connection => connection.CreatedAt);

		if (bankPartnerConnection is null)
		{
			_logger.LogError(
				"No activated bank partner connection found for RTGS Global ID {RtgsGlobalId}",
				signMessageRequest.RtgsGlobalId);

			return NotFound(new { Error = "No activated bank partner connection found, please try again in a few minutes." });
		}

		SignDocumentResponse signDocumentResponse;

		try
		{
			signDocumentResponse = await _jsonSignaturesClient.SignDocumentAsync(
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

	/// <summary>
	/// Endpoint to verify a document / signature.
	/// </summary>
	/// <param name="verifyPrivateSignatureRequest">The data required to verify a message.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="VerifyPrivateSignatureResponse"/></returns>
	[HttpPost("verify")]
	public async Task<IActionResult> Verify(
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
			verified = await _jsonSignaturesClient.VerifyPrivateSignatureAsync(
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

	/// <summary>
	/// Endpoint to verify a document / signature that was signed by the same party.
	/// </summary>
	/// <param name="verifyOwnMessageRequest">The data required to verify a message.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="VerifyOwnMessageResponse"/></returns>
	[HttpPost("verify/own")]
	public async Task<IActionResult> VerifyOwnMessage(VerifyOwnMessageRequest verifyOwnMessageRequest, CancellationToken cancellationToken)
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
				verifyOwnMessageRequest.Message,
				verifyOwnMessageRequest.PublicSignature, publicDid, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Error occurred when sending VerifyPublicSignature request to ID Crypt Cloud Agent");

			throw;
		}

		return Ok(new VerifyOwnMessageResponse { Verified = verified });
	}
}
