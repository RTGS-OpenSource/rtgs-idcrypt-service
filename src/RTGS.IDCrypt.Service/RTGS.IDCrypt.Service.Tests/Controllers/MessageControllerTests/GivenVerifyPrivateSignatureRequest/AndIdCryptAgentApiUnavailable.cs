﻿using System.Text.Json;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Contracts.VerifyMessage;
using RTGS.IDCrypt.Service.Controllers;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Models;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;
using RTGS.IDCrypt.Service.Tests.TestData;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.Wallet;

namespace RTGS.IDCrypt.Service.Tests.Controllers.MessageControllerTests.GivenVerifyPrivateSignatureRequest;

public class AndIdCryptAgentApiUnavailable
{
	private readonly VerifyRequest _request;
	private readonly MessageController _verifyController;
	private readonly FakeLogger<MessageController> _logger = new();

	public AndIdCryptAgentApiUnavailable()
	{
		var message = JsonSerializer.SerializeToElement(new { Message = "I am the walrus" });

		_request = new VerifyRequest
		{
			RtgsGlobalId = "rtgs-global-id-1",
			Message = message,
			PrivateSignature = "private-signature",
			Alias = "alias-1"
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

		jsonSignaturesClientMock
			.Setup(client => client.VerifyPrivateSignatureAsync(
				It.IsAny<JsonElement>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new Exception());

		bankPartnerConnectionsMock.Setup(
			bankPartnerConnections =>
				bankPartnerConnections.GetEnumerator()).Returns(
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
			.Setup(storageTableResolver =>
				storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClientMock.Object);

		var options = Options.Create(new BankPartnerConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_verifyController = new MessageController(
			_logger,
			options,
			storageTableResolverMock.Object,
			jsonSignaturesClientMock.Object,
			Mock.Of<IDateTimeProvider>(),
			Mock.Of<IWalletClient>());
	}

	[Fact]
	public async Task WhenPosting_ThenLog()
	{
		using var _ = new AssertionScope();

		await FluentActions
			.Awaiting(() => _verifyController.Verify(_request))
			.Should()
			.ThrowAsync<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when sending VerifyPrivateSignature request to ID Crypt Cloud Agent"
		});
	}
}
