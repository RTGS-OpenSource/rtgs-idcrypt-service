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
using RTGS.IDCryptSDK.JsonSignatures;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RTGS.IDCrypt.Service.Tests.Controllers.VerifyControllerTests
{
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
				RtgsGlobalId = "rtgs-global-id",
				Message = "message",
				PrivateSignature = "signature", 
				Alias = "alias"
			};

			var matchingBankPartnerConnection = new BankPartnerConnection
			{
				PartitionKey = "rtgs-global-id",
				RowKey = "alias",
				ConnectionId = "connection-id"
			};

			// TODO: RBEN - Do we need these?
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

			// TODO: RBEN - rename variables (add Mock suffix) in other test
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
				new List<BankPartnerConnection>
				{
					matchingBankPartnerConnection,
					nonMatchingBankPartnerConnection1,
					nonMatchingBankPartnerConnection2
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
}
