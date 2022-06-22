using System.Net;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Http;

public record MockHttpResponse
{
	public string Content { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
