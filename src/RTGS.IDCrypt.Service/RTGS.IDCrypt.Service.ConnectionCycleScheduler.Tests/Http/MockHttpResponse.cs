using System.Net;

namespace RTGS.IDCrypt.Service.ConnectionCycleScheduler.Tests.Http;

public record MockHttpResponse
{
	public string Content { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
