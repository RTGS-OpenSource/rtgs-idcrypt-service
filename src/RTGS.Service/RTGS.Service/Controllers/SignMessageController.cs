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
	private readonly IStorageTableResolver _storageTableResolver;
	private readonly IJsonSignaturesClient _jsonSignaturesClient;

	public SignMessageController(
		IStorageTableResolver storageTableResolver,
		IJsonSignaturesClient jsonSignaturesClient)
	{
		_storageTableResolver = storageTableResolver;
		_jsonSignaturesClient = jsonSignaturesClient;
	}

	[HttpPost]
	public async Task<IActionResult> Post(SignMessageRequest signMessageRequest)
	{
		var bankPartnerConnectionsTable = _storageTableResolver.GetTable("bankPartnerConnections");

		var connectionId = bankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Where(x => x.PartitionKey == signMessageRequest.RtgsGlobalId)
			.Where(x => x.RowKey == signMessageRequest.Alias)
			.SingleOrDefault()
			.ConnectionId;

		var signDocumentResponse = await _jsonSignaturesClient.SignJsonDocumentAsync(signMessageRequest.Message, connectionId);

		var signMessageResponse = new SignMessageResponse
		{
			PairwiseDidSignature = signDocumentResponse.PairwiseDidSignature,
			PublicDidSignature = signDocumentResponse.PublicDidSignature,
		};

		return Ok(signMessageResponse);
	}
}
