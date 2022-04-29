using System.Net;
using System.Net.Http;

namespace RTGS.Service.IntegrationTests.Helpers;

public record MockHttpResponse
{
	public HttpContent Content { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
