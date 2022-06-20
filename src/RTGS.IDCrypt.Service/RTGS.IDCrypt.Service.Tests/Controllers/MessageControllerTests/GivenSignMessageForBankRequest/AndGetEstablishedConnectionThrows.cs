using System.Text.Json;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenSignMessageForBankRequest;

public class AndGetEstablishedConnectionThrows
{
	private readonly MessageController _controller;
	private readonly SignMessageForBankRequest _signMessageForBankRequest;

	public AndGetEstablishedConnectionThrows()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });
		var logger = new FakeLogger<MessageController>();

		_signMessageForBankRequest = new SignMessageForBankRequest
		{
			RtgsGlobalId = "rtgs-global-id",
			Message = message
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		var bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();
		bankPartnerConnectionRepositoryMock
			.Setup(repo => repo.GetEstablishedAsync(_signMessageForBankRequest.RtgsGlobalId, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("No active connection found"));

		_controller = new MessageController(
			logger,
			jsonSignaturesClientMock.Object,
			bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IWalletClient>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _controller.SignForBank(_signMessageForBankRequest, default))
			.Should()
			.ThrowAsync<Exception>()
			.WithMessage("No active connection found");
}
