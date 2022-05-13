using System.Net;

namespace RTGS.IDCrypt.Service.IntegrationTests.Helpers;

public record MockHttpResponse
{
	public Func<string> GetContent { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
