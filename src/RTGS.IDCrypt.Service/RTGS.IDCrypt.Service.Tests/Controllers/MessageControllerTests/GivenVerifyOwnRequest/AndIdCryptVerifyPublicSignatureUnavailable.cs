using System.Text.Json;
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

public class AndIdCryptVerifyPublicSignatureUnavailable
{
	private readonly VerifyOwnMessageRequest _request;
	private readonly MessageController _controller;
	private readonly FakeLogger<MessageController> _logger = new();

	public AndIdCryptVerifyPublicSignatureUnavailable()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });
		_request = new VerifyOwnMessageRequest
		{
			Message = message,
			PublicSignature = "public-signature"
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		jsonSignaturesClientMock
			.Setup(client => client.VerifyPublicSignatureAsync(
				It.IsAny<JsonElement>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_controller = new MessageController(
			_logger,
			Mock.Of<IOptions<ConnectionsConfig>>(),
			Mock.Of<IStorageTableResolver>(),
			jsonSignaturesClientMock.Object,
			Mock.Of<IDateTimeProvider>(),
			Mock.Of<IWalletClient>());
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
			"Error occurred when sending VerifyPublicSignature request to ID Crypt Cloud Agent"
		});
	}
}
