using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.SignMessage;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.SignMessageControllerTests.GivenSignMessageRequest;

public class AndIdCryptSignJsonDocumentApiUnavailable
{
	private readonly SignMessageController _signMessageController;
	private readonly SignMessageRequest _signMessageRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private readonly FakeLogger<SignMessageController> _logger;

	public AndIdCryptSignJsonDocumentApiUnavailable()
	{
		_signMessageRequest = new SignMessageRequest
		{
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
		};

		var matchingBankPartnerConnection = new BankPartnerConnection
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id"
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolver = new Mock<IStorageTableResolver>();
		var tableClient = new Mock<TableClient>();
		var bankPartnerConnections = new Mock<Azure.Pageable<BankPartnerConnection>>();

		_jsonSignaturesClientMock
			.Setup(client => client.SignJsonDocumentAsync(
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		bankPartnerConnections.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			new List<BankPartnerConnection>
			{
				matchingBankPartnerConnection,
			}
			.GetEnumerator());

		tableClient.Setup(tableClient =>
			tableClient.Query<BankPartnerConnection>(
				It.IsAny<string>(),
				It.IsAny<int?>(),
				It.IsAny<IEnumerable<string>>(),
				It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnections.Object);

		storageTableResolver
			.Setup(storageTableResolver =>
				storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClient.Object);

		_logger = new FakeLogger<SignMessageController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections",
			GracePeriod = TimeSpan.FromMinutes(5)
		});

		_signMessageController = new SignMessageController(
			_logger,
			options,
			storageTableResolver.Object,
			_jsonSignaturesClientMock.Object,
			new BankPartnerConnectionResolver(options));
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _signMessageController.Post(_signMessageRequest, default))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				"Error occurred when signing JSON document"
			});
	}
}
