using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenVerifyOwnRequest;

public class AndIdCryptAgentApiUnavailable
{
	private readonly VerifyOwnMessageRequest _request;
	private readonly MessageController _controller;
	private readonly FakeLogger<MessageController> _logger = new();

	public AndIdCryptAgentApiUnavailable()
	{
		_request = new VerifyOwnMessageRequest
		{
			Message = @"{ ""Message"": ""I am the walrus"" }",
			PublicSignature = "public-signature"
		};

		var walletClient = new Mock<IWalletClient>();
		walletClient.Setup(client =>
				client.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_controller = new MessageController(
			_logger,
			Mock.Of<IOptions<BankPartnerConnectionsConfig>>(),
			Mock.Of<IStorageTableResolver>(),
			Mock.Of<IJsonSignaturesClient>(),
			Mock.Of<IDateTimeProvider>(),
			walletClient.Object);
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _controller.VerifyOwnMessage(_request, default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent"
		});
	}
}
