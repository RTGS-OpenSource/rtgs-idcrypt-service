using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;
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
	private readonly ConnectionsConfig _connectionsConfig;
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly IJsonSignaturesClient _jsonSignaturesClient;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IWalletClient _walletClient;

	public MessageController(
		ILogger<MessageController> logger,
		IOptions<ConnectionsConfig> connectionsConfig,
		IStorageTableResolver storageTableResolver,
		IJsonSignaturesClient jsonSignaturesClient,
		IDateTimeProvider dateTimeProvider,
		IWalletClient walletClient)
	{
		_logger = logger;
		_connectionsConfig = connectionsConfig.Value;
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
		var bankPartnerConnectionsTable = _storageTableResolver.GetTable(_connectionsConfig.BankPartnerConnectionsTableName);

		var dateThreshold = _dateTimeProvider.UtcNow.Subtract(_connectionsConfig.MinimumConnectionAge);

		var bankPartnerConnections = bankPartnerConnectionsTable
			.Query<BankPartnerConnection>(cancellationToken: cancellationToken)
			.Where(bankPartnerConnection =>
				bankPartnerConnection.PartitionKey == signMessageRequest.RtgsGlobalId
				&& bankPartnerConnection.ActivatedAt <= dateThreshold
				&& bankPartnerConnection.Status == ConnectionStatuses.Active).ToList();

		var bankPartnerConnection = bankPartnerConnections.MaxBy(connection => connection.ActivatedAt);

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
	/// <param name="verifyRequest">The data required to verify a message.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="VerifyResponse"/></returns>
	[HttpPost("verify")]
	public async Task<IActionResult> Verify(
		VerifyRequest verifyRequest,
		CancellationToken cancellationToken = default)
	{
		var bankPartnerConnectionsTable = _storageTableResolver.GetTable(
			_connectionsConfig.BankPartnerConnectionsTableName);

		var bankPartnerConnections = bankPartnerConnectionsTable
			.Query<BankPartnerConnection>(cancellationToken: cancellationToken)
			.Where(bankPartnerConnection =>
				bankPartnerConnection.PartitionKey == verifyRequest.RtgsGlobalId
				&& bankPartnerConnection.RowKey == verifyRequest.Alias
				&& bankPartnerConnection.Status == ConnectionStatuses.Active)
			.ToList();

		if (!bankPartnerConnections.Any())
		{
			_logger.LogError(
				"No bank partner connection found for RTGS Global ID {RtgsGlobalId} and Alias {Alias}",
				verifyRequest.RtgsGlobalId,
				verifyRequest.Alias);

			return NotFound();
		}

		var bankPartnerConnection = bankPartnerConnections.Single();

		bool verified;
		try
		{
			verified = await _jsonSignaturesClient.VerifyPrivateSignatureAsync(
				verifyRequest.Message,
				verifyRequest.PrivateSignature,
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

		var verifyPrivateSignatureResponse = new VerifyResponse
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
			verified = await _jsonSignaturesClient.VerifyPublicSignatureAsync(
				verifyOwnMessageRequest.Message,
				verifyOwnMessageRequest.PublicSignature,
				publicDid,
				cancellationToken);
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
