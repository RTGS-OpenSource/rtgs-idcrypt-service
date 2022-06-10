using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Repositories;
using RTGS.IDCrypt.Service.Services;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Services.ConnectionServiceTests.GivenCycleConnectionForBankRequest;

public class AndIdCryptApiAvailable : IAsyncLifetime
{
	private readonly Mock<IConnectionsClient> _connectionsClientMock = new();
	private readonly Mock<IBankPartnerConnectionRepository> _bankPartnerConnectionRepositoryMock = new();

	private readonly ConnectionService _connectionService;

	private const string RtgsGlobalId = "rtgs-global-id";
	private const string Alias = "alias";

	public AndIdCryptApiAvailable()
	{
		var coreOptions = Options.Create(new CoreConfig
		{
			RtgsGlobalId = "rtgs-global-id"
		});

		var createConnectionInvitationResponse = new CreateConnectionInvitationResponse
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
				Alias,
				It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(createConnectionInvitationResponse)
			.Verifiable();

		var logger = new FakeLogger<ConnectionService>();

		var expectedConnection = new BankPartnerConnection
		{
			PartitionKey = RtgsGlobalId,
			RowKey = Alias,
			Alias = Alias,
			ConnectionId = createConnectionInvitationResponse.ConnectionId,
			Status = "Pending",
			Role = "Inviter"
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
		aliasProviderMock.Setup(provider => provider.Provide()).Returns(Alias);

		_connectionService = new ConnectionService(
			_connectionsClientMock.Object,
			logger,
			_bankPartnerConnectionRepositoryMock.Object,
			Mock.Of<IRtgsConnectionRepository>(),
			aliasProviderMock.Object,
			Mock.Of<IWalletClient>(),
			coreOptions);
	}

	public async Task InitializeAsync() =>
		await _connectionService.CycleConnectionForBankAsync(RtgsGlobalId);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenInvoked_ThenCallCreateConnectionInvitationAsyncWithExpected() => _connectionsClientMock.Verify();

	[Fact]
	public void WhenInvoked_ThenCallSaveBankPartnerConnectionAsyncWithExpected() => _bankPartnerConnectionRepositoryMock.Verify();
}
