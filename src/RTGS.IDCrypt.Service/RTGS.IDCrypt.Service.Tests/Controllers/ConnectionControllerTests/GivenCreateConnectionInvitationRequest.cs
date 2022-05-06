using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests;

public class GivenCreateConnectionInvitationRequest : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock;
	private readonly CreateInvitationResponse _createInvitationResponse;
	private readonly Mock<IWalletClient> _walletClientMock;
	private readonly Mock<IAliasProvider> _mockGuidProvider;
	private readonly ConnectionController _connectionController;
	
	private IActionResult _response;

	public GivenCreateConnectionInvitationRequest()
	{
		const string alias = "alias";
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		const string publicDid = "public-did";

		_connectionsClientMock = new Mock<IConnectionsClient>();

		_createInvitationResponse = new CreateInvitationResponse
		{
			ConnectionId = "connection-id",
			Invitation = new IDCryptSDK.Connections.Models.ConnectionInvitation
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
				alias,
				autoAccept,
				multiUse,
				usePublicDid,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(_createInvitationResponse)
			.Verifiable();

		_walletClientMock = new Mock<IWalletClient>();


		_walletClientMock
			.Setup(walletClient => walletClient.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(publicDid)
			.Verifiable();

		_mockGuidProvider = new Mock<IAliasProvider>();


		_mockGuidProvider
			.Setup(provider => provider.Provide())
			.Returns(alias);

		_connectionController = new ConnectionController(
			_connectionsClientMock.Object, 
			_walletClientMock.Object, 
			_mockGuidProvider.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _connectionController.Post(default);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPosting_ThenReturnOkResponseWithExpected()
	{
		var createConnectionInvitationResponse = new CreateConnectionInvitationResponse
		{
			ConnectionId = "connection-id",
			Alias = "alias",
			AgentPublicDid = "public-did",
			Invitation = new Contracts.Connection.ConnectionInvitation
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

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(createConnectionInvitationResponse);
	}

	[Fact]
	public void WhenPosting_ThenCallCreateInvitationAsyncWithExpected() =>
		_connectionsClientMock.Verify();

	[Fact]
	public void WhenPosting_ThenCallGetPublicDidAsyncWithExpected() =>
		_walletClientMock.Verify();
}
