using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCryptSDK.JsonSignatures;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.VerifyControllerTests;

public class GivenNoMatchingBankPartnerConnectionExists : IAsyncLifetime
{
	private readonly FakeLogger<VerifyController> _logger;
	private readonly VerifyController _controller;
	private readonly VerifyPrivateSignatureRequest _verifyPrivateSignatureRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public GivenNoMatchingBankPartnerConnectionExists()
	{
		// TODO: RBEN - variations of RtgsGlobalId and Alias
		_verifyPrivateSignatureRequest = new VerifyPrivateSignatureRequest
		{
			RtgsGlobalId = "rtgs-global-id",
			Message = "message",
			PrivateSignature = "signature",
			Alias = "alias"
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
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		bankPartnerConnectionsMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			new List<BankPartnerConnection>
			{
				nonMatchingBankPartnerConnection1,
				nonMatchingBankPartnerConnection2,
				nonMatchingBankPartnerConnection3
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
			.Setup(storageTableResolver => storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClientMock.Object);

		_logger = new FakeLogger<VerifyController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_controller = new VerifyController(
			_logger,
			options,
			storageTableResolverMock.Object,
			_jsonSignaturesClientMock.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _controller.PrivateSignature(_verifyPrivateSignatureRequest);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenDoNotCallVerifyPrivateSignatureAsync() =>
		_jsonSignaturesClientMock.Verify(client =>
			client.VerifyPrivateSignatureAsync(
				It.IsAny<string>(), 
				It.IsAny<string>(), 
				It.IsAny<string>(), 
				It.IsAny<CancellationToken>()),
			Times.Never);

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenReturnNotFoundResponse() =>
		_response.Should().BeOfType<NotFoundResult>();

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenLog() =>
		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
			{
				$"No bank partner connection found for RTGS Global ID {_verifyPrivateSignatureRequest.RtgsGlobalId} " +
				$"and Alias {_verifyPrivateSignatureRequest.Alias}"
			});
}
