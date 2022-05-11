using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

namespace RTGS.IDCrypt.Service.Tests.Controllers.VerifyControllerTests.GivenVerifyPrivateSignatureRequest;

public class AndNoMatchingBankPartnerConnectionExists
{
	private readonly FakeLogger<VerifyController> _logger;
	private readonly VerifyController _controller;
	private readonly Mock<IJsonSignaturesClient> _jsonSignaturesClientMock;

	public AndNoMatchingBankPartnerConnectionExists()
	{
		_jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		var tableClientMock = new Mock<TableClient>();
		var bankPartnerConnectionsMock = new Mock<Azure.Pageable<BankPartnerConnection>>();

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

	[Theory]
	[InlineData("rtgs-global-id-1", "alias-3")]
	[InlineData("rtgs-global-id-2", "alias-1")]
	[InlineData("rtgs-global-id-3", "alias-5")]
	public async Task ThenMessageIsNotVerified(string rtgsGlobalId, string alias)
	{
		var response = await _controller.Post(new VerifyPrivateSignatureRequest
		{
			RtgsGlobalId = rtgsGlobalId,
			Message = "message",
			PrivateSignature = "signature",
			Alias = alias
		});

		using var _ = new AssertionScope();

		_jsonSignaturesClientMock.Verify(client =>
				client.VerifyPrivateSignatureAsync(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<CancellationToken>()),
			Times.Never);

		response.Should().BeOfType<NotFoundResult>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			$"No bank partner connection found for RTGS Global ID {rtgsGlobalId} " +
			$"and Alias {alias}"
		});
	}
}
