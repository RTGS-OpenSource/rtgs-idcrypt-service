using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RTGS.Service.Dtos;
using RTGS.Service.IntegrationTests.Fixtures;
using Xunit;

namespace RTGS.Service.IntegrationTests.Controllers.SignMessageController;

public class GivenNoMatchingBankPartnerConnectionExists : IClassFixture<TestFixture>
{
	private readonly HttpClient _client;

	public GivenNoMatchingBankPartnerConnectionExists(TestFixture testFixture)
	{
		var application = new TestWebApplicationFactory(testFixture);

		_client = application.CreateClient();
	}

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
