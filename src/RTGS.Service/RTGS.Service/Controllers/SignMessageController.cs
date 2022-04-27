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
	public IActionResult Post(SignMessageRequest signMessageRequest)
	{
		var bankPartnerConnectionsTable = _storageTableResolver.GetTable("bankPartnerConnections");

		var x = bankPartnerConnectionsTable
			.Query<BankPartnerConnection>()
			.Where(x => x.Alias == signMessageRequest.Alias);

		return Ok();
	}
}
