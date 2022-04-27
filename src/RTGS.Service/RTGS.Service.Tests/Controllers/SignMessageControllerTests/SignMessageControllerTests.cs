using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Moq;
using RTGS.IDCryptSDK.JsonSignatures;
using RTGS.IDCryptSDK.JsonSignatures.Models;
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
	public async Task WhenPostingSignMessageRequest_AndBankConnectionExists_ThenSignMessageIsCalledWithExpected()
	{
		var signMessageRequest = new SignMessageRequest
		{
			Alias = "alias",
			Message = "message",
			RtgsGlobalId = "rtgs-global-id"
		};

		var bankPartnerConnection = new BankPartnerConnection()
		{
			PartitionKey = "rtgs-global-id",
			RowKey = "alias",
			ConnectionId = "connection-id"
		};

		var signDocumentResponse = new SignDocumentResponse
		{
			PairwiseDidSignature = "pairwise-did-signature",
			PublicDidSignature = "public-did-signature"
		};

		var jsonSignaturesClientMock = new Mock<IJsonSignaturesClient>();
		var storageTableResolver = new Mock<IStorageTableResolver>();
		var tableClient = new Mock<TableClient>();
		var bankPartnerConnections = new Mock<Azure.Pageable<BankPartnerConnection>>();

		jsonSignaturesClientMock
			.Setup(client => client.SignJsonDocumentAsync(signMessageRequest.Message, bankPartnerConnection.ConnectionId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(signDocumentResponse)
			.Verifiable();

		bankPartnerConnections.Setup(bankPartnerConnections => bankPartnerConnections.GetEnumerator()).Returns(
			new List<BankPartnerConnection>() { bankPartnerConnection }.GetEnumerator());

		tableClient.Setup(tableClient =>
			tableClient.Query<BankPartnerConnection>(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
			.Returns(bankPartnerConnections.Object);

		storageTableResolver
			.Setup(storageTableResolver => storageTableResolver.GetTable("bankPartnerConnections"))
			.Returns(tableClient.Object);

		var controller = new SignMessageController(
			storageTableResolver.Object,
			jsonSignaturesClientMock.Object);
		
		await controller.Post(signMessageRequest);

		jsonSignaturesClientMock.Verify();
	}
}
