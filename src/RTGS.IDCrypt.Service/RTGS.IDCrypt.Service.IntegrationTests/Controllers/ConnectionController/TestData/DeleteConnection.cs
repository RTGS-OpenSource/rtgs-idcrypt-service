using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.ConnectionController.TestData;

public static class DeleteConnection
{
	public const string Path = "/connections/connection-id-1";

	public static HttpRequestResponseContext HttpRequestResponseContext => new(Path, string.Empty);
}
