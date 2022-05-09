using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures;
using Xunit;

namespace RTGS.IDCrypt.Service.IntegrationTests.Middleware;

public class GivenExceptionThrown : IClassFixture<ThrowingFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _httpResponse;

	public GivenExceptionThrown(ThrowingFixture testFixture)
	{
		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() =>
		_httpResponse = await _client.PostAsJsonAsync("api/signmessage", ThrowingFixture.SignMessageRequest);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public async Task ThenReturnExpected500Response()
	{
		using var _ = new AssertionScope();

		_httpResponse.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

		_httpResponse.Content.Headers.ContentType.Should().Be(MediaTypeHeaderValue.Parse("application/json"));

		var content = await _httpResponse.Content.ReadAsStringAsync();

		content.Should().Be("{\"error\":\"testing middleware with controller\"}");
	}
}
