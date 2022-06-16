using System.Net;
using System.Net.Http;

namespace RTGS.IDCrypt.Service.Scheduler.Tests.Http;

public record MockHttpResponse
{
	public HttpContent Content { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
