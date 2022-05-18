using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Webhooks.IdCryptConnectionMessageHandler.TestData;

internal static class SendProofRequest
{
	public const string Path = "/present-proof/send-request";

	private static string SerialisedResponse => $@"{{ 
	}}";

	public static HttpRequestResponseContext HttpRequestResponseContext =>
		new(Path, SerialisedResponse);
}
