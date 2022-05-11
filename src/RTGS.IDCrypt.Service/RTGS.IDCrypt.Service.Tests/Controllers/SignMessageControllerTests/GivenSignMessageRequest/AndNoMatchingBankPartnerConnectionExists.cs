using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
using RTGS.IDCrypt.Service.Tests.TestData;
using RTGS.IDCryptSDK.JsonSignatures;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.SignMessageControllerTests.GivenSignMessageRequest;

public class AndNoMatchingBankPartnerConnectionExists : IAsyncLifetime
{
	private readonly FakeLogger<SignMessageController> _logger;
	private readonly SignMessageController _controller;
	private readonly SignMessageRequest _signMessageRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public AndNoMatchingBankPartnerConnectionExists()
	{
		_signMessageRequest = new SignMessageRequest
		{
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
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

		_logger = new FakeLogger<SignMessageController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections",
			MinimumConnectionAge = TimeSpan.FromMinutes(5)
		});

		var bankPartnerConnectionResolverMock = new Mock<IBankPartnerConnectionResolver>();
		bankPartnerConnectionResolverMock.Setup(
				resolver => resolver.Resolve(It.IsAny<List<BankPartnerConnection>>()))
			.Returns((BankPartnerConnection)null);

		_controller = new SignMessageController(
			_logger,
			options,
			storageTableResolverMock.Object,
			_jsonSignaturesClientMock.Object,
			new BankPartnerConnectionResolver(options));
	}

	public async Task InitializeAsync() =>
		_response = await _controller.Post(_signMessageRequest, default);

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
				$"No bank partner connection found for RTGS Global ID {_signMessageRequest.RtgsGlobalId}"
			});
}
