using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.Message.Sign;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenSignMessageRequest;

public class AndIdCryptAgentUnavailable
{
	private readonly MessageController _controller;
	private readonly SignMessageRequest _signMessageRequest;
	private readonly FakeLogger<MessageController> _logger = new();

	public AndIdCryptAgentUnavailable()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		_signMessageRequest = new SignMessageRequest
		{
			RtgsGlobalId = "rtgs-global-id",
			Message = message
		};

		var referenceDate = new DateTime(2022, 4, 1, 0, 0, 0);
		var dateTimeProviderMock = new Mock<IDateTimeProvider>();
		dateTimeProviderMock.SetupGet(provider => provider.UtcNow).Returns(referenceDate);

		var matchingBankPartnerConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id",
			CreatedAt = referenceDate.Subtract(TimeSpan.FromDays(1))
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		jsonSignaturesClientMock
			.Setup(client => client.SignDocumentAsync(
				It.IsAny<JsonElement>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		bankPartnerConnectionsMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			new List<BankPartnerConnection>
			{
				matchingBankPartnerConnection,
			}
			.GetEnumerator());

		tableClientMock.Setup(tableClient =>
			tableClient.Query<BankPartnerConnection>(
				It.IsAny<string>(),
				It.IsAny<int?>(),
				It.IsAny<IEnumerable<string>>(),
				It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnectionsMock.Object);

		storageTableResolverMock
			.Setup(storageTableResolver =>
				storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClientMock.Object);

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections",
			MinimumConnectionAge = TimeSpan.FromMinutes(5)
		});

		_controller = new MessageController(
			_logger,
			options,
			storageTableResolverMock.Object,
			jsonSignaturesClientMock.Object,
			dateTimeProviderMock.Object,
			Mock.Of<IWalletClient>());
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _controller.Sign(_signMessageRequest, default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				"Error occurred when signing JSON document"
			});
	}
}
