using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

namespace RTGS.IDCrypt.Service.IntegrationTests.Controllers.BankConnectionController.GivenObsoleteConnectionIdsRequest;

public class AndConnectionsExist : IClassFixture<ObsoleteConnectionIdsConnectionFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private HttpResponseMessage _httpResponse;

	public AndConnectionsExist(ObsoleteConnectionIdsConnectionFixture testFixture)
	{
		_client = testFixture.CreateClient();
	}

	public async Task InitializeAsync() => _httpResponse = await _client.GetAsync("api/bank-connection/ObsoleteConnectionIds");

	public Task DisposeAsync() => Task.CompletedTask;

	[Fact]
	public void ThenReturnOk() =>
		_httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

	[Fact]
	public async Task ThenReturnCorrectIds()
	{
		var rtgsGlobalIds = await _httpResponse.Content.ReadFromJsonAsync<string[]>();

		rtgsGlobalIds.Should().BeEquivalentTo(new List<string>
		{
			"connection-id-1",
			"connection-id-2",
			"connection-id-8"
		});
	}
}
