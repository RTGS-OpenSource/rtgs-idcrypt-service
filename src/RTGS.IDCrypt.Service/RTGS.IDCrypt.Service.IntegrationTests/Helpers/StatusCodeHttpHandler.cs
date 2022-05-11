using System.Net;
using System.Net.Http;

namespace RTGS.IDCrypt.Service.IntegrationTests.Helpers;

public class StatusCodeHttpHandler : DelegatingHandler
{
	private readonly Dictionary<string, MockHttpResponse> _mockHttpResponses;

	public Dictionary<string, IList<HttpRequestMessage>> Requests { get; }

	private StatusCodeHttpHandler(Dictionary<string, MockHttpResponse> mockHttpResponses)
	{
		Requests = new Dictionary<string, IList<HttpRequestMessage>>();
		_mockHttpResponses = mockHttpResponses;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var requestPath = request.RequestUri!.LocalPath;

		if (!Requests.ContainsKey(requestPath))
		{
			Requests[requestPath] = new List<HttpRequestMessage>();
		}
		Requests[requestPath].Add(request);

		var responseMock = _mockHttpResponses[requestPath];

		var response = new HttpResponseMessage(responseMock.HttpStatusCode)
		{
			Content = new StringContent(responseMock.Content)
		};

		response.RequestMessage = request;

		return Task.FromResult(response);
	}

	public void Reset() => Requests.Clear();

	public sealed class Builder
	{
		private Dictionary<string, MockHttpResponse> Responses { get; } = new();

		public static Builder Create() => new();

		public Builder WithServiceUnavailableResponse(string path) =>
			WithResponse(path, null, HttpStatusCode.ServiceUnavailable);

		public Builder WithOkResponse(HttpRequestResponseContext httpRequestResponseContext) =>
			WithResponse(
				httpRequestResponseContext.RequestPath,
				httpRequestResponseContext.ResponseContent,
				HttpStatusCode.OK);

		public StatusCodeHttpHandler Build() => new(Responses);

		private Builder WithResponse(string path, string content, HttpStatusCode statusCode)
		{
			var mockResponse = new MockHttpResponse
			{
				HttpStatusCode = statusCode,
				Content = content
			};

			Responses[path] = mockResponse;

			return this;
		}
	}
}
