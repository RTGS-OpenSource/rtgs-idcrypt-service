using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.Service.Config;
using RTGS.Service.Controllers;
using RTGS.Service.Dtos;
using RTGS.Service.Models;
using RTGS.Service.Storage;
using RTGS.Service.Tests.Logging;
using Xunit;

namespace RTGS.Service.Tests.Controllers.SignMessageControllerTests;

public class GivenNoMatchingBankPartnerConnectionExists : IAsyncLifetime
{
	private readonly FakeLogger<SignMessageController> _logger;
	private readonly SignMessageController _controller;
	private readonly SignMessageRequest _signMessageRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public GivenNoMatchingBankPartnerConnectionExists()
	{
		_signMessageRequest = new SignMessageRequest
		{
			Alias = "alias",
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
		};

		var nonMatchingBankPartnerConnection1 = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-1",
			RowKey = "alias-1",
			ConnectionId = "connection-id-1"
		};

		var nonMatchingBankPartnerConnection2 = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-2",
			RowKey = "alias-2",
			ConnectionId = "connection-id-2"
		};

		var nonMatchingBankPartnerConnection3 = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id-3",
			RowKey = "alias-3",
			ConnectionId = "connection-id-3"
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolver = new Mock<IStorageTableResolver>();
		var tableClient = new Mock<TableClient>();
		var bankPartnerConnections = new Mock<Azure.Pageable<BankPartnerConnection>>();

		bankPartnerConnections.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			new List<BankPartnerConnection>
			{
				nonMatchingBankPartnerConnection1,
				nonMatchingBankPartnerConnection2,
				nonMatchingBankPartnerConnection3
			}
			.GetEnumerator());

		tableClient.Setup(tableClient =>
			tableClient.Query<BankPartnerConnection>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnections.Object);

		storageTableResolver
			.Setup(storageTableResolver => storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClient.Object);

		_logger = new FakeLogger<SignMessageController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_controller = new SignMessageController(
			_logger,
			options,
			storageTableResolver.Object,
			_jsonSignaturesClientMock.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _controller.Post(_signMessageRequest);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingSignMessageRequest_ThenDoNotCallSignMessage() =>
		_jsonSignaturesClientMock.Verify(client =>
			client.SignJsonDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

	[Fact]
	public void WhenPostingSignMessageRequest_ThenReturnNotFoundResponse() =>
		_response.Should().BeOfType<NotFoundResult>();

	[Fact]
	public void WhenPostingSignMessageRequest_ThenLog() =>
		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				$"No bank partner connection found for alias {_signMessageRequest.Alias} and RTGS Global ID {_signMessageRequest.RtgsGlobalId}"
			});
}
