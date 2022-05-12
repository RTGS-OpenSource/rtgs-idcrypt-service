using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.VerifyControllerTests.GivenVerifyPublicSignatureRequest;

public class AndIdCryptAgentApiUnavailable
{
	private readonly VerifyPublicSignatureRequest _request;
	private readonly VerifyController _verifyController;
	private readonly FakeLogger<VerifyController> _logger;

	public AndIdCryptAgentApiUnavailable()
	{
		_request = new VerifyPublicSignatureRequest
		{
			Message = @"{ ""Message"": ""I am the walrus"" }",
			PublicSignature = "public-signature"
		};

		var walletClient = new Mock<IWalletClient>();
		walletClient.Setup(client =>
				client.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_logger = new FakeLogger<VerifyController>();

		_verifyController = new VerifyController(
			_logger,
			Mock.Of<IOptions<BankPartnerConnectionsConfig>>(),
			Mock.Of<IStorageTableResolver>(),
			Mock.Of<IJsonSignaturesClient>(),
			walletClient.Object);
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _verifyController.VerifyPublicSignature(_request, default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent"
		});
	}
}
