using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Moq;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.Service.Controllers;
using RTGS.Service.Dtos;
using RTGS.Service.Models;
using RTGS.Service.Storage;
using Xunit;

namespace RTGS.Service.Tests.Controllers.SignMessageControllerTests;

public class SignMessageControllerTests
{
	public SignMessageControllerTests()
	{

	}

	[Fact]
	public async Task WhenPostingSignMessageRequest_ThenSignMessageIsCalledWithExpected()
	{
		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolver = new Mock<IStorageTableResolver>();

		var tableClient = new Mock<TableClient>();
		var bankPartnerConnections = new Mock<Azure.Pageable<BankPartnerConnection>>();

		bankPartnerConnections.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			new List<BankPartnerConnection>()
			{
				new BankPartnerConnection()
				{
					PartitionKey = "rtgs-global-id",
					RowKey = "alias",
					ConnectionId = "connection-id"
				}
			}.GetEnumerator());

		tableClient.Setup(tableClient =>
			tableClient.Query<BankPartnerConnection>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnections.Object);

		storageTableResolver
			.Setup(storageTableResolver => storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClient.Object);

		var controller = new SignMessageController(
			storageTableResolver.Object,
			jsonSignaturesClientMock.Object);

		var signMessageRequest = new SignMessageRequest
		{
			Alias = "alias",
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
		};

		await controller.Post(signMessageRequest);
	}
}
