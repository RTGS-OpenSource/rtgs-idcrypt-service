using Moq;
using RTGS.IDCrypt.Service.Contracts.Message.Verify;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenVerifyRequest;

public class AndGetActiveConnectionThrows
{
	private readonly MessageController _controller;

	public AndGetActiveConnectionThrows()
	{
		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();

		var bankPartnerConnectionRepositoryMock = new Mock<IBankPartnerConnectionRepository>();
		bankPartnerConnectionRepositoryMock.Setup(connection =>
				connection.GetActiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception("No active connection found"));

		_controller = new MessageController(
			new FakeLogger<MessageController>(),
			jsonSignaturesClientMock.Object,
			bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			Mock.Of<IWalletClient>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _controller.Verify(new VerifyRequest()))
			.Should()
			.ThrowAsync<Exception>()
			.WithMessage("No active connection found");
}
