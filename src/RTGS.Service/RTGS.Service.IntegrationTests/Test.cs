using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RTGS.Service.Dtos;
using RTGS.Service.IntegrationTests.Fixture;
using Xunit;

namespace RTGS.Service.IntegrationTests;

[Collection("TestCollection")]
public class Test 
{
	private readonly TestFixture _testFixture;

	public Test(TestFixture testFixture)
	{
		_testFixture = testFixture;
	}

	[Fact]
	public async Task HelloWorldTest()
	{
		var application = new TestWebApplicationFactory(_testFixture);

		var client = application.CreateClient();

		var signMessageRequest = new SignMessageRequest
		{
			Alias = "alias",
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
		};

		var response = await client.PostAsync(
			"api/signmessage",
			new StringContent(
				JsonSerializer.Serialize(signMessageRequest),
				Encoding.UTF8,
				MediaTypeNames.Application.Json));
	}
}
