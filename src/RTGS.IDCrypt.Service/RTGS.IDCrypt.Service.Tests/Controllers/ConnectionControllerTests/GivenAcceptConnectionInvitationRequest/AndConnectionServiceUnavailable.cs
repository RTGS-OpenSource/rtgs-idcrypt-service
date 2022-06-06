using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenAcceptConnectionInvitationRequest;

public class AndConnectionServiceUnavailable
{
	private readonly ConnectionController _connectionController;

	public AndConnectionServiceUnavailable()
	{
		var connectionServiceMock = new Mock<IConnectionService>();

		connectionServiceMock
			.Setup(service => service.AcceptInvitationAsync(
				It.IsAny<Models.ConnectionInvitation>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		_connectionController = new ConnectionController(connectionServiceMock.Object, Mock.Of<IBankPartnerConnectionRepository>());
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
