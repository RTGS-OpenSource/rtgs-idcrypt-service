using Moq;
using RTGS.IDCrypt.Service.Helpers;
using RTGS.IDCrypt.Service.IntegrationTests.Extensions;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.IntegrationTests.Fixtures.Connection;

public class ObsoleteConnectionIdsConnectionFixture : ConnectionsTestFixtureBase
{
	private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();

	private readonly DateTime _referenceDate =
		DateTime.SpecifyKind(new(2022, 5, 1, 0, 0, 0), DateTimeKind.Utc);

	public ObsoleteConnectionIdsConnectionFixture()
	{
		_dateTimeProviderMock.SetupGet(provider => provider.UtcNow)
			.Returns(_referenceDate);
	}

	protected override async Task Seed()
	{
		var activatedDate = DateTime.SpecifyKind(new(2022, 4, 2, 0, 0, 0), DateTimeKind.Utc);
		var createdDate = DateTime.SpecifyKind(new(2022, 4, 1, 0, 0, 0), DateTimeKind.Utc);

		var connections = new List<BankPartnerConnection>()
		{
			// 1 active with 2 stale (role is inviter) - for rtgs-global-id-1
			new()
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-1",
				ConnectionId = "connection-id-1",
				Alias = "alias-1",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate.Subtract(TimeSpan.FromMinutes(2)),
				PublicDid = "public-did-1",
				Status = "Active",
				Role = "Inviter"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-2",
				ConnectionId = "connection-id-2",
				Alias = "alias-2",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate.Subtract(TimeSpan.FromMinutes(1)),
				PublicDid = "public-did-1",
				Status = "Active",
				Role = "Inviter"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-1",
				RowKey = "alias-3",
				ConnectionId = "connection-id-3",
				Alias = "alias-3",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate,
				PublicDid = "public-did-1",
				Status = "Active",
				Role = "Inviter"
			},

			// 1 active with 2 stale (role is inviter) - for rtgs-global-id-2
			new()
			{
				PartitionKey = "rtgs-global-id-2",
				RowKey = "alias-2-1",
				ConnectionId = "connection-id-2-1",
				Alias = "alias-2-1",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate.Subtract(TimeSpan.FromMinutes(2)),
				PublicDid = "public-did-1",
				Status = "Active",
				Role = "Inviter"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-2",
				RowKey = "alias-2-2",
				ConnectionId = "connection-id-2-2",
				Alias = "alias-2-2",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate.Subtract(TimeSpan.FromMinutes(1)),
				PublicDid = "public-did-1",
				Status = "Active",
				Role = "Inviter"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-2",
				RowKey = "alias-2-3",
				ConnectionId = "connection-id-2-3",
				Alias = "alias-2-3",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate,
				PublicDid = "public-did-1",
				Status = "Active",
				Role = "Inviter"
			},

			// 1 active with 2 stale (role is invitee)
			new()
			{
				PartitionKey = "rtgs-global-id-2",
				RowKey = "alias-4",
				ConnectionId = "connection-id-4",
				Alias = "alias-4",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate.Subtract(TimeSpan.FromMinutes(2)),
				PublicDid = "public-did-2",
				Status = "Active",
				Role = "Invitee"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-2",
				RowKey = "alias-5",
				ConnectionId = "connection-id-5",
				Alias = "alias-5",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate.Subtract(TimeSpan.FromMinutes(1)),
				PublicDid = "public-did-2",
				Status = "Active",
				Role = "Invitee"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-2",
				RowKey = "alias-6",
				ConnectionId = "connection-id-6",
				Alias = "alias-6",
				CreatedAt = createdDate,
				ActivatedAt = activatedDate,
				PublicDid = "public-did-2",
				Status = "Active",
				Role = "Invitee"
			},

			// 1 pending within expiry threshold, 1 pending outside expiry threshold (role is inviter)
			new()
			{
				PartitionKey = "rtgs-global-id-3",
				RowKey = "alias-7",
				ConnectionId = "connection-id-7",
				Alias = "alias-7",
				CreatedAt = _referenceDate.Subtract(new TimeSpan(0,0,4,59)),
				ActivatedAt = null,
				PublicDid = "public-did-3",
				Status = "Pending",
				Role = "Inviter"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-3",
				RowKey = "alias-8",
				ConnectionId = "connection-id-8",
				Alias = "alias-8",
				CreatedAt = _referenceDate.Subtract(new TimeSpan(0,0,5,01)),
				ActivatedAt = null,
				PublicDid = "public-did-3",
				Status = "Pending",
				Role = "Inviter"
			},

			// 1 pending within expiry threshold, 1 pending outside expiry threshold (role is invitee)
			new()
			{
				PartitionKey = "rtgs-global-id-4",
				RowKey = "alias-9",
				ConnectionId = "connection-id-9",
				Alias = "alias-9",
				CreatedAt = _referenceDate.Subtract(new TimeSpan(0,0,4,59)),
				ActivatedAt = null,
				PublicDid = "public-did-4",
				Status = "Pending",
				Role = "Invitee"
			},
			new()
			{
				PartitionKey = "rtgs-global-id-5",
				RowKey = "alias-10",
				ConnectionId = "connection-id-10",
				Alias = "alias-10",
				CreatedAt = _referenceDate.Subtract(new TimeSpan(0,0,5,01)),
				ActivatedAt = null,
				PublicDid = "public-did-5",
				Status = "Pending",
				Role = "Invitee"
			},
		};

		foreach (var connection in connections)
		{
			await InsertBankPartnerConnectionAsync(connection);
		}
	}

	protected override void CustomiseHost(IHostBuilder builder) =>
		builder.ConfigureServices(services => services.AddDateTimeProvider(_dateTimeProviderMock.Object));
}
