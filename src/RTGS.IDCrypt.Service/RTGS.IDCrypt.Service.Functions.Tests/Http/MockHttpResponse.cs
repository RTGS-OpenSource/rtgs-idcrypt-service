using System.Net;
using System.Net.Http;

namespace RTGS.IDCrypt.Service.Function.Tests.Http;

public record MockHttpResponse
{
	public HttpContent Content { get; init; }
	public HttpStatusCode HttpStatusCode { get; init; }
}
