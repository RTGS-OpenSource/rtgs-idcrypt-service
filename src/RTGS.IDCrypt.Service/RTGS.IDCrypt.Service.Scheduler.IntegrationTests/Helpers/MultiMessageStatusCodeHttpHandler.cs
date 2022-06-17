using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTGS.IDCrypt.Service.Scheduler.IntegrationTests.Helpers;

public class MultiMessageStatusCodeHttpHandler : DelegatingHandler
{
	private readonly Dictionary<string, MockHttpResponse> _mockHttpResponses;

	public Dictionary<string, HttpRequestMessage> Requests { get; }

	public MultiMessageStatusCodeHttpHandler(Dictionary<string, MockHttpResponse> mockHttpResponses)
	{
		Requests = new Dictionary<string, HttpRequestMessage>();
		_mockHttpResponses = mockHttpResponses;
	}

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var requestPath = request.RequestUri!.LocalPath;

		Requests[requestPath] = request;

		var responseMock = _mockHttpResponses[requestPath];

		var response = new HttpResponseMessage(responseMock.HttpStatusCode);

		if (responseMock.Content is not null)
		{
			response.Content = new StringContent(responseMock.Content);
		}

		response.RequestMessage = request;

		return Task.FromResult(response);
	}
}
