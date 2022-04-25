using Microsoft.AspNetCore.Mvc;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.Service.Dtos;

namespace RTGS.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SignMessageController : ControllerBase
{
	private IJsonSignaturesClient _jsonSignaturesClient;

	public SignMessageController(IJsonSignaturesClient jsonSignaturesClient)
	{
		_jsonSignaturesClient = jsonSignaturesClient;
	}

	[HttpPost]
	public IActionResult Post(SignMessageRequest signMessageRequest)
	{
		return Ok();
	}
}
