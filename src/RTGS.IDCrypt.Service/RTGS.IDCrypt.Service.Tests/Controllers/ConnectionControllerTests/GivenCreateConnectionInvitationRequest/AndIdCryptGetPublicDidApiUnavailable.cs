using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndIdCryptGetPublicDidApiUnavailable
{
	private readonly FakeLogger<ConnectionController> _logger;
	private readonly Mock<IConnectionsClient> _connectionsClientMock;
	private readonly CreateInvitationResponse _createInvitationResponse;
	private readonly Mock<IWalletClient> _walletClientMock;
	private readonly Mock<IAliasProvider> _mockAliasProvider;
	private readonly ConnectionController _connectionController;
	private const string Alias = "alias";

	public AndIdCryptGetPublicDidApiUnavailable()
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		_connectionsClientMock = new Mock<IConnectionsClient>();

		_createInvitationResponse = new CreateInvitationResponse
		{
			ConnectionId = "connection-id",
			Invitation = new ConnectionInvitation
			{
				Id = "id",
				Type = "type",
				Label = "label",
				RecipientKeys = new[]
				{
					"recipient-key-1"
				},
				ServiceEndpoint = "service-endpoint"
			}
		};

		_connectionsClientMock
			.Setup(connectionsClient => connectionsClient.CreateInvitationAsync(
				Alias,
				autoAccept,
				multiUse,
				usePublicDid,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(_createInvitationResponse)
			.Verifiable();

		_walletClientMock = new Mock<IWalletClient>();

		_walletClientMock
			.Setup(walletClient => walletClient.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		_mockAliasProvider = new Mock<IAliasProvider>();

		_mockAliasProvider
			.Setup(provider => provider.Provide())
			.Returns(Alias);

		_logger = new FakeLogger<ConnectionController>();

		_connectionController = new ConnectionController(
			_logger,
			_connectionsClientMock.Object,
			_walletClientMock.Object,
			_mockAliasProvider.Object);
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _connectionController.Post(default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				"Error occurred when sending GetPublicDid request to ID Crypt Cloud Agent"
			});
	}
}
