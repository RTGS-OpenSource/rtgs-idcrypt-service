using System.Threading.Tasks;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.IDCrypt.Service.IntegrationTests.Controllers.SignMessageController.TestData;
using RTGS.IDCrypt.Service.IntegrationTests.Helpers;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures;

public class NoMatchingBankPartnerConnectionFixture : BankPartnerTestFixtureBase
{
	public NoMatchingBankPartnerConnectionFixture()
	{
		IdCryptStatusCodeHttpHandler = StatusCodeHttpHandler.Builder
			.Create()
			.WithOkResponse(SignDocument.HttpRequestResponseContext)
			.Build();
	}

	public static SignMessageRequest SignMessageRequest => new()
	{
		RtgsGlobalId = "rtgs-global-id",
		Message = @"{ ""Message"": ""I am the walrus"" }"
	};

	public override Task Seed() =>
		Task.CompletedTask;
}
