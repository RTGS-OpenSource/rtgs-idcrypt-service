using RTGS.IDCrypt.Service.IntegrationTests.Helpers;
using RTGS.IDCryptSDK.JsonSignatures.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.MessageController.Sign.TestData;

public static class SignDocument
{
	public const string Path = "/json-signatures/sign";

	public static SignDocumentResponse ExpectedResponse => new()
	{
		PairwiseDidSignature = "pairwise-did-signature",
		PublicDidSignature = "public-did-signature",
	};

	private static string SerialisedResponse =>
		$@"{{
			""pairwise_did_signature"": ""{ExpectedResponse.PairwiseDidSignature}"",
			""public_did_signature"": ""{ExpectedResponse.PublicDidSignature}""
		}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
