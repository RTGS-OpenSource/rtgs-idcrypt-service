using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Connection;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.BasicMessage;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenCycleConnectionForBankRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock = new();
	private readonly Mock<IBasicMessageClient> _basicMessageClientMock = new();

	private readonly ConnectionService _connectionService;
	private const string PartnerRtgsGlobalId = "partner-rtgs-global-id";

	public AndIdCryptApiAvailable()
	{
		const string publicDid = "public-did";

		var config = new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		};

		const string alias = "alias";

		var coreOptions = Options.Create(config);

		var createConnectionInvitationResponse = new IDCryptSDK.Connections.Models.CreateConnectionInvitationResponse
		{
			ConnectionId = "connection-id",
			Alias = "alias",
			InvitationUrl = "invitation-url",
			Invitation = new IDCryptSDK.Connections.Models.ConnectionInvitation
			{
				Id = "id",
				Type = "type",
				Label = "label",
				ImageUrl = "image-url",
				RecipientKeys = new[]
				{
					"recipient-key-1"
				},
				ServiceEndpoint = "service-endpoint",
				Did = "did"
			}
		};

		_connectionsClientMock
			.Setup(client => client.CreateConnectionInvitationAsync(
				alias,
				It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(createConnectionInvitationResponse)
			.Verifiable();

		var logger = new FakeLogger<ConnectionService>();

		var expectedConnection = new BankPartnerConnection
		{
			PartitionKey = PartnerRtgsGlobalId,
			RowKey = alias,
			Alias = alias,
			ConnectionId = createConnectionInvitationResponse.ConnectionId,
			Status = "Pending",
			Role = "Inviter",
			PublicDid = publicDid
		};

		Func<BankPartnerConnection, bool> connectionMatches = actualConnection =>
		{
			actualConnection.Should().BeEquivalentTo(expectedConnection, options =>
			{
				options.Excluding(connection => connection.ETag);
				options.Excluding(connection => connection.Timestamp);

				return options;
			});

			return true;
		};

		_bankPartnerConnectionRepositoryMock.Setup(repo => repo.CreateAsync(
				It.Is<BankPartnerConnection>(connection => connectionMatches(connection)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		var aliasProviderMock = new Mock<IAliasProvider>();
		aliasProviderMock.Setup(provider => provider.Provide()).Returns(alias);

		var establishedBankConnection = new BankPartnerConnection
		{
			PartitionKey = PartnerRtgsGlobalId,
			RowKey = "established-alias",
			Alias = "established-alias",
			ConnectionId = "established-connection-id",
		};

		_bankPartnerConnectionRepositoryMock.Setup(repo => repo.GetEstablishedAsync(
				PartnerRtgsGlobalId,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(establishedBankConnection)
			.Verifiable();

		var expectedMessage = new Models.ConnectionInvitation
		{
			Alias = createConnectionInvitationResponse.Alias,
			Did = createConnectionInvitationResponse.Invitation.Did,
			FromRtgsGlobalId = config.RtgsGlobalId,
			Id = createConnectionInvitationResponse.Invitation.Id,
			ImageUrl = createConnectionInvitationResponse.Invitation.ImageUrl,
			InvitationUrl = createConnectionInvitationResponse.InvitationUrl,
			Label = createConnectionInvitationResponse.Invitation.Label,
			RecipientKeys = createConnectionInvitationResponse.Invitation.RecipientKeys,
			ServiceEndpoint = createConnectionInvitationResponse.Invitation.ServiceEndpoint,
			Type = createConnectionInvitationResponse.Invitation.Type,
			PublicDid = publicDid
		};

		Func<Models.ConnectionInvitation, bool> messageMatches = actualMessage =>
		{
			actualMessage.Should().BeEquivalentTo(expectedMessage);

			return true;
		};

		var walletClientMock = new Mock<IWalletClient>();

		walletClientMock
			.Setup(client => client.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(publicDid);

		_basicMessageClientMock
			.Setup(client => client.SendAsync(
				establishedBankConnection.ConnectionId,
				nameof(CycleConnectionRequest),
				It.Is<Models.ConnectionInvitation>(message => messageMatches(message)),
				It.IsAny<CancellationToken>()))
			.Verifiable();

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			logger,
			_bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			aliasProviderMock.Object,
			walletClientMock.Object,
			coreOptions,
			_basicMessageClientMock.Object);
	}

	public async Task InitializeAsync() =>
		await _connectionService.CycleConnectionForBankAsync(PartnerRtgsGlobalId);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenCallCreateConnectionInvitationAsyncWithExpected() => _connectionsClientMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallSaveBankPartnerConnectionAsyncWithExpected() => _bankPartnerConnectionRepositoryMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallSendBasicMessageWithExpected() => _basicMessageClientMock.Verify();
}
