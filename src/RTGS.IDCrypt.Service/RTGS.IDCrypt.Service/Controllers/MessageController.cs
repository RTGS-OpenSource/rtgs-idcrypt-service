using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
	private readonly ILogger<MessageController> _logger;
	private readonly IJsonSignaturesClient _jsonSignaturesClient;
	private readonly IBankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly IRtgsConnectionRepository _rtgsConnectionRepository;
	private readonly IWalletClient _walletClient;

	public MessageController(
		ILogger<MessageController> logger,
		IJsonSignaturesClient jsonSignaturesClient,
		IBankPartnerConnectionRepository bankPartnerConnectionRepository,
		IRtgsConnectionRepository rtgsConnectionRepository,
		IWalletClient walletClient)
	{
		_logger = logger;
		_jsonSignaturesClient = jsonSignaturesClient;
		_bankPartnerConnectionRepository = bankPartnerConnectionRepository;
		_rtgsConnectionRepository = rtgsConnectionRepository;
		_walletClient = walletClient;
	}

	// TODO remove this method once sdk/e2e/simulators updated to use new sign/for-bank
	[HttpPost("sign")]
	[Obsolete("Use SignForBank instead")]
	public async Task<IActionResult> Sign(
		SignMessageForBankRequest signMessageRequest,
		CancellationToken cancellationToken) =>
		await SignForBank(signMessageRequest, cancellationToken);

	/// <summary>
	/// Endpoint to sign a document where the intended recipient is a bank.
	/// </summary>
	/// <param name="signMessageForBankRequest">The data required to sign a message.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="SignDocumentResponse"/></returns>
	[HttpPost("sign/for-bank")]
	public async Task<IActionResult> SignForBank(
		SignMessageForBankRequest signMessageForBankRequest,
		CancellationToken cancellationToken)
	{
		var connection =
			await _bankPartnerConnectionRepository.GetEstablishedAsync(signMessageForBankRequest.RtgsGlobalId, cancellationToken);

		var signMessageResponse = await Sign(signMessageForBankRequest.Message, connection.ConnectionId, connection.Alias,
			cancellationToken);

		return Ok(signMessageResponse);
	}

	/// <summary>
	/// Endpoint to sign a document where the intended recipient is RTGS.
	/// </summary>
	/// <param name="signMessageForRtgsRequest">The data required to sign a message.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be cancelled.</param>
	/// <returns><see cref="SignDocumentResponse"/></returns>
	[HttpPost("sign/for-rtgs")]
	public async Task<IActionResult> SignForRtgs(
		SignMessageForRtgsRequest signMessageForRtgsRequest,
		CancellationToken cancellationToken)
	{
		var connection =
			await _rtgsConnectionRepository.GetEstablishedAsync(cancellationToken);

		var signMessageResponse = await Sign(signMessageForRtgsRequest.Message, connection.ConnectionId, connection.Alias,
			cancellationToken);

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
		var bankPartnerConnection =
			await _bankPartnerConnectionRepository.GetActiveAsync(verifyRequest.RtgsGlobalId, verifyRequest.Alias, cancellationToken);

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

	private async Task<SignMessageResponse> Sign(JsonElement message, string connectionId, string alias, CancellationToken cancellationToken)
	{
		SignDocumentResponse signDocumentResponse;

		try
		{
			signDocumentResponse = await _jsonSignaturesClient.SignDocumentAsync(
				message,
				connectionId,
				cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred when signing JSON document");

			throw;
		}

		return new SignMessageResponse
		{
			PairwiseDidSignature = signDocumentResponse.PairwiseDidSignature,
			PublicDidSignature = signDocumentResponse.PublicDidSignature,
			Alias = alias
		};
	}
}
