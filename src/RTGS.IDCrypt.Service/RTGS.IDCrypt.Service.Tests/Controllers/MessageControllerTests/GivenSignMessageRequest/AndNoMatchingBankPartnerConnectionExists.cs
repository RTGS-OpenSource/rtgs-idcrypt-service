using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
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
using RTGS.IDCrypt.Service.Tests.TestData;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenSignMessageRequest;

public class AndNoMatchingBankPartnerConnectionExists : IAsyncLifetime
{
	private readonly FakeLogger<MessageController> _logger = new();
	private readonly MessageController _controller;
	private readonly SignMessageRequest _signMessageRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public AndNoMatchingBankPartnerConnectionExists()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		_signMessageRequest = new SignMessageRequest
		{
			RtgsGlobalId = "rtgs-global-id",
			Message = message
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		bankPartnerConnectionsMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			TestBankPartnerConnections.Connections
			.GetEnumerator());

		tableClientMock.Setup(tableClient =>
			tableClient.Query<BankPartnerConnection>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnectionsMock.Object);

		storageTableResolverMock
			.Setup(storageTableResolver => storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClientMock.Object);

		var options = Options.Create(new ConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections",
			MinimumConnectionAge = TimeSpan.FromMinutes(5)
		});

		var referenceDate = new DateTime(2022, 4, 1, 0, 0, 0);
		var dateTimeProviderMock = new Mock<IDateTimeProvider>();
		dateTimeProviderMock.SetupGet(provider => provider.UtcNow).Returns(referenceDate);

		_controller = new MessageController(
			_logger,
			options,
			storageTableResolverMock.Object,
			_jsonSignaturesClientMock.Object,
			dateTimeProviderMock.Object,
			Mock.Of<IWalletClient>());
	}

	public async Task InitializeAsync() =>
		_response = await _controller.Sign(_signMessageRequest, default);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingSignMessageRequest_ThenDoNotCallSignMessage() =>
		_jsonSignaturesClientMock.Verify(client =>
			client.SignDocumentAsync(It.IsAny<JsonElement>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

	[Fact]
	public void WhenPostingSignMessageRequest_ThenReturnNotFoundResponse() =>
		_response.Should().BeOfType<NotFoundObjectResult>();

	[Fact]
	public void WhenPostingSignMessageRequest_ThenLog() =>
		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				$"No activated bank partner connection found for RTGS Global ID {_signMessageRequest.RtgsGlobalId}"
			});
}
