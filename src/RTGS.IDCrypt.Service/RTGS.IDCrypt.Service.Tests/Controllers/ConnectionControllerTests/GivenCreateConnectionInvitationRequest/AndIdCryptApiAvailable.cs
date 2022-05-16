using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock;
	private readonly CreateInvitationResponse _createInvitationResponse;
	private readonly Mock<IWalletClient> _walletClientMock;
	private readonly ConnectionController _connectionController;
	private readonly Mock<TableClient> _tableClientMock;
	private const string Alias = "alias";
	private const string PublicDid = "public-did";

	private IActionResult _response;
	private Mock<IStorageTableResolver> _storageTableResolverMock;

	public AndIdCryptApiAvailable()
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

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
			.ReturnsAsync(PublicDid)
			.Verifiable();

		var mockAliasProvider = new Mock<IAliasProvider>();

		mockAliasProvider
			.Setup(provider => provider.Provide())
			.Returns(Alias);

		var logger = new FakeLogger<ConnectionController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections",
			PendingBankPartnerConnectionsTableName = "pendingBankPartnerConnections"
		});

		var expectedPendingConnection = new PendingBankPartnerConnection
		{
			PartitionKey = _createInvitationResponse.ConnectionId,
			RowKey = Alias,
			ConnectionId = _createInvitationResponse.ConnectionId,
			Alias = Alias
		};

		Func<PendingBankPartnerConnection, bool> connectionMatches = request =>
		{
			request.Should().BeEquivalentTo(expectedPendingConnection, options =>
			{
				options.Excluding(connection => connection.ETag);
				options.Excluding(connection => connection.Timestamp);

				return options;
			});

			return true;
		};

		_tableClientMock = new Mock<TableClient>();
		_tableClientMock
			.Setup(tableClient => tableClient.AddEntityAsync(
				It.Is<PendingBankPartnerConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_storageTableResolverMock = new Mock<IStorageTableResolver>();
		_storageTableResolverMock
			.Setup(resolver => resolver.GetTable("pendingBankPartnerConnections"))
			.Returns(_tableClientMock.Object)
			.Verifiable();

		_connectionController = new ConnectionController(
			logger,
			_connectionsClientMock.Object,
			_walletClientMock.Object,
			mockAliasProvider.Object,
			_storageTableResolverMock.Object,
			options);
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
			ConnectionId = _createInvitationResponse.ConnectionId,
			Alias = Alias,
			AgentPublicDid = PublicDid,
			Invitation = new Contracts.Connection.ConnectionInvitation
			{
				Id = _createInvitationResponse.Invitation.Id,
				Type = _createInvitationResponse.Invitation.Type,
				Label = _createInvitationResponse.Invitation.Label,
				RecipientKeys = _createInvitationResponse.Invitation.RecipientKeys,
				ServiceEndpoint = _createInvitationResponse.Invitation.ServiceEndpoint
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

	[Fact]
	public void WhenPosting_ThenWritePendingBankPartnerToTableStorage() =>
		_tableClientMock.Verify();
}
