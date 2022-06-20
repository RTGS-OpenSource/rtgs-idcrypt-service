using System.Text.Json;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenSignMessageForRtgsRequest;

public class AndGetEstablishedConnectionThrows
{
	private readonly MessageController _controller;
	private readonly SignMessageForRtgsRequest _signMessageForRtgsRequest;

	public AndGetEstablishedConnectionThrows()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });
		var logger = new FakeLogger<MessageController>();

		_signMessageForRtgsRequest = new SignMessageForRtgsRequest
		{
			Message = message
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		var rtgsConnectionRepositoryMock = new Mock<IRtgsConnectionRepository>();
		rtgsConnectionRepositoryMock
			.Setup(repo => repo.GetEstablishedAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("No active connection found"));

		var options = Options.Create(new ConnectionsConfig());

		_controller = new MessageController(
			logger,
			options,
			Mock.Of<IStorageTableResolver>(),
			jsonSignaturesClientMock.Object,
			Mock.Of<IBankPartnerConnectionRepository>(),
			rtgsConnectionRepositoryMock.Object,
			Mock.Of<IWalletClient>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _controller.SignForRtgs(_signMessageForRtgsRequest, default))
			.Should()
			.ThrowAsync<Exception>()
			.WithMessage("No active connection found");
}
