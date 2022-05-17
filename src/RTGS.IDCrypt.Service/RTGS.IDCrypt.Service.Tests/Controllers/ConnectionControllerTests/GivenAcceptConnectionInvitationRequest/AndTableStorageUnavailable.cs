using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenAcceptConnectionInvitationRequest;

public class AndTableStorageUnavailable
{
	private readonly ConnectionController _connectionController;
	private readonly AcceptConnectionInvitationRequest _request;
	private readonly FakeLogger<ConnectionController> _logger;

	public AndTableStorageUnavailable()
	{
		var connectionResponse = new ConnectionResponse
		{
			Alias = "alias",
			ConnectionId = "connection-id",
			State = "invitation"
		};

		var expectedRequest = new ReceiveAndAcceptInvitationRequest
		{
			Alias = "alias",
			Id = "id",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint"
		};

		Func<ReceiveAndAcceptInvitationRequest, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(expectedRequest);

			return true;
		};

		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
				It.Is<ReceiveAndAcceptInvitationRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(connectionResponse)
			.Verifiable();

		var connectionStorageServiceMock = new Mock<IConnectionStorageService>();
		connectionStorageServiceMock.Setup(service =>
				service.SavePendingBankPartnerConnectionAsync(It.IsAny<PendingBankPartnerConnection>(),
					It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		_logger = new FakeLogger<ConnectionController>();

		_connectionController = new ConnectionController(
			_logger,
			Mock.Of<IWalletClient>(),
			Mock.Of<IAliasProvider>(),
			Mock.Of<IConnectionService>(),
			Mock.Of<IConnectionStorageService>());

		_request = new AcceptConnectionInvitationRequest
		{
			Alias = "alias",
			Id = "id",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint",
			AgentPublicDid = "agent-public-did"
		};
	}

	[Fact]
	public async Task WhenPosting_ThenThrowException() =>
		await FluentActions
			.Awaiting(() => _connectionController.Accept(_request, default))
			.Should()
			.ThrowAsync<Exception>();
}
