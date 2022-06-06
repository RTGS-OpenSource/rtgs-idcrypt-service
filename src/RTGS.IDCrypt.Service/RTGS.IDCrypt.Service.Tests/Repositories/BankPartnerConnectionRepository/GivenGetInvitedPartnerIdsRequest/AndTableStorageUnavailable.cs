using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.Storage;
using RTGS.IDCrypt.Service.Tests.Logging;

namespace RTGS.IDCrypt.Service.Tests.Repositories.BankPartnerConnectionRepository.GivenGetInvitedPartnerIdsRequest;

public class AndTableStorageUnavailable
{
	private readonly Service.Repositories.BankPartnerConnectionRepository _bankPartnerConnectionRepository;
	private readonly FakeLogger<Service.Repositories.BankPartnerConnectionRepository> _logger = new();

	public AndTableStorageUnavailable()
	{
		var storageTableResolverMock = new Mock<IStorageTableResolver>();
		storageTableResolverMock
			.Setup(resolver => resolver.GetTable("bankPartnerConnections"))
			.Throws<Exception>();

		var options = Options.Create(new ConnectionsConfig
		{
			BankPartnerConnectionsTableName = "bankPartnerConnections"
		});

		_bankPartnerConnectionRepository = new Service.Repositories.BankPartnerConnectionRepository(
			storageTableResolverMock.Object,
			options,
			_logger,
			Mock.Of<IDateTimeProvider>());
	}

	[Fact]
	public void WhenInvoked_ThenThrows() => FluentActions
		.Invoking(() => _bankPartnerConnectionRepository.GetInvitedPartnerIds())
		.Should()
		.Throw<Exception>();

	[Fact]
	public void WhenInvoked_ThenLogs()
	{
		using var _ = new AssertionScope();

		FluentActions
			.Invoking(() => _bankPartnerConnectionRepository.GetInvitedPartnerIds())
			.Should()
			.Throw<Exception>();

		_logger.Logs[LogLevel.Error].Should().BeEquivalentTo(new List<string>
		{
			"Error occurred when querying bank partner connections"
		});
	}
}
