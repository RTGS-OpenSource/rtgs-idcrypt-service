﻿using Microsoft.Extensions.Logging;
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

public class AndIdCryptVerifyPublicSignatureUnavailable
{
	private readonly VerifyOwnMessageRequest _request;
	private readonly VerifyController _verifyController;
	private readonly FakeLogger<VerifyController> _logger;

	public AndIdCryptVerifyPublicSignatureUnavailable()
	{
		_request = new VerifyOwnMessageRequest
		{
			Message = @"{ ""Message"": ""I am the walrus"" }",
			PublicSignature = "public-signature"
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		jsonSignaturesClientMock
			.Setup(client => client.VerifyJsonDocumentPublicSignatureAsync(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_logger = new FakeLogger<VerifyController>();

		_verifyController = new VerifyController(
			_logger,
			Mock.Of<IOptions<BankPartnerConnectionsConfig>>(),
			Mock.Of<IStorageTableResolver>(),
			jsonSignaturesClientMock.Object,
			Mock.Of<IWalletClient>());
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _verifyController.VerifyOwnMessage(_request, default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when sending VerifyPublicSignature request to ID Crypt Cloud Agent"
		});
	}
}
