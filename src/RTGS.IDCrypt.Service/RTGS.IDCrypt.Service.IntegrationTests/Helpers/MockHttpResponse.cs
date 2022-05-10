using System.Net;

namespace RTGS.IDCrypt.Service.IntegrationTests.Helpers;

public record MockHttpResponse
{
	public string Content { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
