using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
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
using RTGS.IDCryptSDK.JsonSignatures.Models;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.SignMessageControllerTests.GivenSignMessageRequest;

public class AndMatchingBankPartnerConnectionExists : IAsyncLifetime
{
	private readonly SignMessageController _controller;
	private readonly SignMessageRequest _signMessageRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public AndMatchingBankPartnerConnectionExists()
	{
		DateTimeOffsetServer.Init(() => new DateTimeOffset(2022, 1, 1, 0, 0, 0, new TimeSpan()));

		_signMessageRequest = new SignMessageRequest
		{
			Message = "message",
			RtgsGlobalId = "rtgs-global-id-1"
		};

		var signDocumentResponse = new SignDocumentResponse
		{
			PairwiseDidSignature = "pairwise-did-signature",
			PublicDidSignature = "public-did-signature"
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		_jsonSignaturesClientMock
			.Setup(client => client.SignJsonDocumentAsync(
				_signMessageRequest.Message,
				TestBankPartnerConnections.GetConnectionId(_signMessageRequest.RtgsGlobalId),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(signDocumentResponse)
			.Verifiable();

		bankPartnerConnectionMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			TestBankPartnerConnections.Connections
			.GetEnumerator());

		tableClientMock.Setup(tableClient =>
			tableClient.Query<BankPartnerConnection>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnectionMock.Object);

		storageTableResolverMock
			.Setup(storageTableResolver => storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClientMock.Object);

		var logger = new FakeLogger<SignMessageController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections",
			MinimumConnectionAge = TimeSpan.FromMinutes(5)
		});

		var bankPartnerConnectionResolverMock = new Mock<IBankPartnerConnectionResolver>();
		bankPartnerConnectionResolverMock.Setup(
				resolver => resolver.Resolve(It.IsAny<List<BankPartnerConnection>>()))
			.Returns(TestBankPartnerConnections.Connections.First());

		_controller = new SignMessageController(
			logger,
			options,
			storageTableResolverMock.Object,
			_jsonSignaturesClientMock.Object,
			bankPartnerConnectionResolverMock.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _controller.Post(_signMessageRequest, default);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingSignMessageRequest_ThenCallSignMessageWithExpected() =>
		_jsonSignaturesClientMock.Verify();

	[Fact]
	public void WhenPostingSignMessageRequest_ThenReturnOkResponseWithExpectedSignatures()
	{
		var signMessageResponse = new SignMessageResponse
		{
			PairwiseDidSignature = "pairwise-did-signature",
			PublicDidSignature = "public-did-signature",
			Alias = "alias-1"
		};

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(signMessageResponse);
	}
}
