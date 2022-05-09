﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Tests.TestData;
using RTGS.IDCryptSDK.JsonSignatures;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.VerifyControllerTests;

public class GivenMatchingBankPartnerConnectionExists : IAsyncLifetime
{
	private readonly VerifyController _controller;
	private readonly VerifyPrivateSignatureRequest _verifyPrivateSignatureRequest;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;
	private IActionResult _response;

	public GivenMatchingBankPartnerConnectionExists()
	{
		_verifyPrivateSignatureRequest = new VerifyPrivateSignatureRequest
		{
			RtgsGlobalId = "rtgs-global-id-1",
			Message = "message",
			PrivateSignature = "signature",
			Alias = "alias-1"
		};

		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		_jsonSignaturesClientMock
			.Setup(client => client.VerifyPrivateSignatureAsync(
				_verifyPrivateSignatureRequest.Message,
				_verifyPrivateSignatureRequest.PrivateSignature,
				_verifyPrivateSignatureRequest.Alias,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(true)
			.Verifiable();

		bankPartnerConnectionsMock.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			TestBankPartnerConnections.Connections
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

		var logger = new FakeLogger<VerifyController>();

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_controller = new VerifyController(
			logger,
			options,
			storageTableResolverMock.Object,
			_jsonSignaturesClientMock.Object);
	}

	public async Task InitializeAsync() =>
		_response = await _controller.PrivateSignature(_verifyPrivateSignatureRequest);

	public Task DisposeAsync() =>
		Task.CompletedTask;

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenCallVerifyPrivateSignatureWithExpected() =>
		_jsonSignaturesClientMock.Verify();

	[Fact]
	public void WhenPostingVerifyPrivateSignatureRequest_ThenReturnOkResponseWithVerifiedTrue()
	{
		var verifyPrivateSignatureResponse = new VerifyPrivateSignatureResponse
		{
			Verified = true
		};

		_response.Should().BeOfType<OkObjectResult>()
			.Which.Value.Should().BeEquivalentTo(verifyPrivateSignatureResponse);
	}
}
