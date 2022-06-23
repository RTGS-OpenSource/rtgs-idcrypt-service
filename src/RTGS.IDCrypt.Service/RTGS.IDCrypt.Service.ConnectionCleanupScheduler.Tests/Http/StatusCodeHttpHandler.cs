using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace RTGS.IDCrypt.Service.ConnectionCleanupScheduler.Tests.Http;

public sealed class StatusCodeHttpHandler : DelegatingHandler
{
	private readonly List<KeyValuePair<MockHttpRequest, MockHttpResponse>> _mockHttpResponses;

	public ConcurrentDictionary<MockHttpRequest, ConcurrentBag<HttpRequestMessage>> Requests { get; }

	private StatusCodeHttpHandler(List<KeyValuePair<MockHttpRequest, MockHttpResponse>> mockHttpResponses)
	{
		Requests = new ConcurrentDictionary<MockHttpRequest, ConcurrentBag<HttpRequestMessage>>();
		_mockHttpResponses = mockHttpResponses;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var requestPath = request.RequestUri!.LocalPath;

		var mockRequest = new MockHttpRequest(request.Method, requestPath);

		Requests.TryAdd(mockRequest, new ConcurrentBag<HttpRequestMessage>());
		Requests[mockRequest].Add(request);

		var responseMock = _mockHttpResponses
			.Single(pair => pair.Key.Method == request.Method &&
				new Regex(pair.Key.Path).IsMatch(requestPath))
			.Value;

		var response = new HttpResponseMessage(responseMock.HttpStatusCode)
		{
			Content = new StringContent(responseMock.Content),
			RequestMessage = request
		};

		return Task.FromResult(response);
	}

	public sealed class Builder
	{
		private List<KeyValuePair<MockHttpRequest, MockHttpResponse>> Responses { get; } = new();

		public static Builder Create() => new();

		public Builder WithServiceUnavailableResponse(MockHttpRequest request) =>
			WithResponse(request, null, HttpStatusCode.ServiceUnavailable);

		public Builder WithOkResponse(HttpRequestResponseContext httpRequestResponseContext) =>
			WithResponse(
				httpRequestResponseContext.Request,
				httpRequestResponseContext.ResponseContent,
				HttpStatusCode.OK);

		public Builder WithNotFoundResponse(MockHttpRequest request) =>
			WithResponse(request, null, HttpStatusCode.NotFound);

		public StatusCodeHttpHandler Build() => new(Responses);

		private Builder WithResponse(MockHttpRequest request, string content, HttpStatusCode statusCode)
		{
			var mockResponse = new MockHttpResponse
			{
				HttpStatusCode = statusCode,
				Content = content
			};

			Responses.Add(new KeyValuePair<MockHttpRequest, MockHttpResponse>(request, mockResponse));

			return this;
		}
	}
}
