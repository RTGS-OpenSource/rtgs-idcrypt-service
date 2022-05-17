using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenAcceptConnectionInvitationRequest;

public class AndIdCryptApiUnavailable
{
	private readonly FakeLogger<ConnectionController> _logger;
	private readonly ConnectionController _connectionController;

	public AndIdCryptApiUnavailable()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
				It.IsAny<ReceiveAndAcceptInvitationRequest>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		_logger = new FakeLogger<ConnectionController>();

		_connectionController = new ConnectionController(
			_logger,
			Mock.Of<IWalletClient>(),
			Mock.Of<IAliasProvider>(),
			connectionServiceMock.Object,
			Mock.Of<IConnectionStorageService>());
	}

	[Fact]
	public async Task WhenPosting_ThenThrows()
	{
		using var _ = new AssertionScope();

		var request = new AcceptConnectionInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint",
			AgentPublicDid = "agent-public-did"
		};

		await FluentActions
			.Awaiting(() => _connectionController.Accept(request, default))
			.Should()
			.ThrowAsync<Exception>();
	}
}
