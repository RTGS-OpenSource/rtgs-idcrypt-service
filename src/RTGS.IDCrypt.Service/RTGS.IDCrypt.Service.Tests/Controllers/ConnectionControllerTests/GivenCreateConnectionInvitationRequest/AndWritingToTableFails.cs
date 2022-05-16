using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.Connections;
using RTGS.IDCryptSDK.Connections.Models;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.ConnectionControllerTests.GivenCreateConnectionInvitationRequest;

public class AndWritingToTableFails
{
	private readonly ConnectionController _connectionController;
	private readonly FakeLogger<ConnectionController> _logger;
	private readonly Mock<TableClient> _tableClientMock;

	public AndWritingToTableFails()
	{
		const bool autoAccept = true;
		const bool multiUse = false;
		const bool usePublicDid = false;

		var connectionsClientMock = new Mock<IConnectionsClient>();

		var createInvitationResponse = new CreateInvitationResponse
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

		var alias = "alias";

		connectionsClientMock
			.Setup(connectionsClient => connectionsClient.CreateInvitationAsync(
				alias,
				autoAccept,
				multiUse,
				usePublicDid,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(createInvitationResponse);

		var walletClientMock = new Mock<IWalletClient>();

		var publicDid = "public-did";

		walletClientMock
			.Setup(walletClient => walletClient.GetPublicDidAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(publicDid);

		var mockAliasProvider = new Mock<IAliasProvider>();

		mockAliasProvider
			.Setup(provider => provider.Provide())
			.Returns(alias);

		_logger = new FakeLogger<ConnectionController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections",
			PendingBankPartnerConnectionsTableName = "pendingBankPartnerConnections"
		});

		_tableClientMock = new Mock<TableClient>();
		_tableClientMock
			.Setup(tableClient => tableClient.AddEntityAsync(
				It.IsAny<PendingBankPartnerConnection>(),
				It.IsAny<CancellationToken>()))
			.Throws<Exception>();

		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		storageTableResolverMock
			.Setup(resolver => resolver.GetTable("pendingBankPartnerConnections"))
			.Returns(_tableClientMock.Object);

		_connectionController = new ConnectionController(
			_logger,
			connectionsClientMock.Object,
			walletClientMock.Object,
			mockAliasProvider.Object,
			storageTableResolverMock.Object,
			options);
	}

	[Fact]
	public async Task WhenPosting_ThenThrowException() =>
		await FluentActions
			.Awaiting(() => _connectionController.Post(default))
			.Should()
			.ThrowAsync<Exception>();

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
			"Error occurred when saving pending bank partner connection"
		});
	}
}
