using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Verify.TestData;

public static class VerifyPrivateSignature
{
	public const string Path = "/json-signatures/verify/connection-did";

	private static VerifyResponse ExpectedResponse => new()
	{
		Verified = true
	};

	private static string SerialisedResponse =>
		$@"{{
			""verified"": {ExpectedResponse.Verified.ToString().ToLowerInvariant()}
		}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
