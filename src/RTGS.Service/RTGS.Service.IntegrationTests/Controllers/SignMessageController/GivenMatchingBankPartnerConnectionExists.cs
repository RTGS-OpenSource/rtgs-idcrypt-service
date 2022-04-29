using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RTGS.Service.Dtos;
using RTGS.Service.IntegrationTests.Fixtures;
using RTGS.Service.Models;
using Xunit;

namespace RTGS.Service.IntegrationTests.Controllers.SignMessageController;

public class GivenMatchingBankPartnerConnectionExists : IClassFixture<TestFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly TestFixture _testFixture;

	public GivenMatchingBankPartnerConnectionExists(TestFixture testFixture)
	{
		_testFixture = testFixture;

		var application = new TestWebApplicationFactory(testFixture);

		_client = application.CreateClient();
	}

	public async Task InitializeAsync()
	{
		var bankPartnerConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id"
		};

		await _testFixture.InsertBankPartnerConnectionAsync(bankPartnerConnection);
	}

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public async Task TestTest()
	{

		var signMessageRequest = new SignMessageRequest
		{
			Alias = "alias",
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
		};

		var response = await _client.PostAsync(
			"api/signmessage",
			new StringContent(
				JsonSerializer.Serialize(signMessageRequest),
				Encoding.UTF8,
				MediaTypeNames.Application.Json));
	}

	[Fact]
	public async Task TestTestTest()
	{
		var signMessageRequest = new SignMessageRequest
		{
			Alias = "alias",
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
		};

		var response = await _client.PostAsync(
			"api/signmessage",
			new StringContent(
				JsonSerializer.Serialize(signMessageRequest),
				Encoding.UTF8,
				MediaTypeNames.Application.Json));
	}
}
