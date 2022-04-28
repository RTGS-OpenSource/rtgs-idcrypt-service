using Microsoft.AspNetCore.Mvc;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.Service.Dtos;
using RTGS.Service.Models;
using RTGS.Service.Storage;

namespace RTGS.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignMessageController : ControllerBase
{
	private readonly ILogger<SignMessageController> _logger;
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly IJsonSignaturesClient _jsonSignaturesClient;

	public SignMessageController(
		ILogger<SignMessageController> logger,
		IStorageTableResolver storageTableResolver,
		IJsonSignaturesClient jsonSignaturesClient)
	{
		_logger = logger;
		_storageTableResolver = storageTableResolver;
		_jsonSignaturesClient = jsonSignaturesClient;
	}

	[HttpPost]
	public async Task<IActionResult> Post(SignMessageRequest signMessageRequest)
	{
		var bankPartnerConnectionsTable = _storageTableResolver.GetTable("bankPartnerConnections");

		var bankPartnerConnections = bankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Where(bankPartnerConnection =>
				bankPartnerConnection.PartitionKey == signMessageRequest.RtgsGlobalId &&
				bankPartnerConnection.RowKey == signMessageRequest.Alias)
			.ToList();

		if (!bankPartnerConnections.Any())
		{
			_logger.LogError(
				"No bank partner connection found for alias {Alias} and RTGS Global ID {RtgsGlobalId}",
				signMessageRequest.Alias,
				signMessageRequest.RtgsGlobalId);

			return NotFound();
		}

		if (bankPartnerConnections.Count > 1)
		{
			_logger.LogError(
				"More than one bank partner connection found for alias {Alias} and RTGS Global ID {RtgsGlobalId}",
				signMessageRequest.Alias,
				signMessageRequest.RtgsGlobalId);

			return StatusCode(
				StatusCodes.Status500InternalServerError,
				"More than one bank partner connection found for given alias and RTGS Global ID");
		}

		var signDocumentResponse = await _jsonSignaturesClient.SignJsonDocumentAsync(signMessageRequest.Message, bankPartnerConnections.First().ConnectionId);

		var signMessageResponse = new SignMessageResponse
		{
			PairwiseDidSignature = signDocumentResponse.PairwiseDidSignature,
			PublicDidSignature = signDocumentResponse.PublicDidSignature,
		};

		return Ok(signMessageResponse);
	}
}
