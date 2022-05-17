using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionService.GivenAcceptInvitationRequest;

public class AndIdCryptApiUnavailable
{
	private readonly Service.Services.ConnectionService _connectionService;
	private readonly ReceiveAndAcceptInvitationRequest _request;
	private readonly FakeLogger<Service.Services.ConnectionService> _logger;


	public AndIdCryptApiUnavailable()
	{
		var connectionsClientMock = new Mock<IConnectionsClient>();

		_request = new ReceiveAndAcceptInvitationRequest
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint"
		};

		Func<ReceiveAndAcceptInvitationRequest, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(_request);

			return true;
		};

		connectionsClientMock
			.Setup(client => client.ReceiveAndAcceptInvitationAsync(
				It.Is<ReceiveAndAcceptInvitationRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>()
			.Verifiable();

		_logger = new FakeLogger<Service.Services.ConnectionService>();

		_connectionService = new Service.Services.ConnectionService(
			connectionsClientMock.Object,
			_logger);
	}

	[Fact]
	public async Task WhenInvoked_ThenThrows() =>
		await FluentActions
			.Awaiting(() => _connectionService.AcceptInvitationAsync(_request))
			.Should()
			.ThrowAsync<Exception>();

	[Fact]
	public async Task WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionService.AcceptInvitationAsync(_request))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo("Error occurred when accepting invitation");
	}
}
