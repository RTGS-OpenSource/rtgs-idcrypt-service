using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenAcceptInvitationRequest;

public class AndIdCryptApiUnavailable
{
	private readonly ConnectionService _connectionService;
	private readonly Models.ConnectionInvitation _request;
	private readonly FakeLogger<ConnectionService> _logger;

	public AndIdCryptApiUnavailable()
	{
		var connectionsClientMock = new Mock<IConnectionsClient>();

		_request = new Models.ConnectionInvitation
		{
			Id = "id",
			Type = "type",
			Alias = "alias",
			Label = "label",
			RecipientKeys = new[] { "recipient-key" },
			ServiceEndpoint = "service-endpoint",
			InvitationUrl = "invitation-url",
			Did = "did",
			ImageUrl = "image-url",
			PublicDid = "public-did"
		};

		Func<ReceiveAndAcceptInvitationRequest, bool> requestMatches = request =>
		{
			request.Should().BeEquivalentTo(_request, options =>
			{
				options.Excluding(connection => connection.PublicDid);
				options.Excluding(connection => connection.ImageUrl);
				options.Excluding(connection => connection.Did);
				options.Excluding(connection => connection.InvitationUrl);

				return options;
			});

			return true;
		};

		connectionsClientMock
			.Setup(client => client.ReceiveAndAcceptInvitationAsync(
				It.Is<ReceiveAndAcceptInvitationRequest>(request => requestMatches(request)),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>()
			.Verifiable();

		_logger = new FakeLogger<ConnectionService>();

		_connectionService = new ConnectionService(
			connectionsClientMock.Object,
			_logger,
			Mock.Of<IConnectionRepository>(),
			Mock.Of<IAliasProvider>(),
			Mock.Of<IWalletClient>());
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
