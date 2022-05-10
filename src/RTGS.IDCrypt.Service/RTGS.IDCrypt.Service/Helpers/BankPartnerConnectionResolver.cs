using Microsoft.Extensions.Options;
using RTGS.IDCrypt.Service.Config;
using RTGS.IDCrypt.Service.Models;

namespace RTGS.IDCrypt.Service.Helpers;

public class BankPartnerConnectionResolver : IBankPartnerConnectionResolver
{
	private readonly BankPartnerConnectionsConfig _bankPartnerConnectionsConfig;

	public BankPartnerConnectionResolver(
		IOptions<BankPartnerConnectionsConfig> bankPartnerConnectionsConfig)
	{
		_bankPartnerConnectionsConfig = bankPartnerConnectionsConfig.Value;
	}

	public BankPartnerConnection Resolve(List<BankPartnerConnection> bankPartnerConnections)
	{
		ArgumentNullException.ThrowIfNull(bankPartnerConnections);

		switch (bankPartnerConnections.Count)
		{
			case 0:
				return null;
			case 1:
				return bankPartnerConnections.Single();
			default:
				return SelectConnection(bankPartnerConnections);
		}
	}

	private BankPartnerConnection SelectConnection(
		List<BankPartnerConnection> bankPartnerConnections)
	{
		var connectionsPastOrAtGracePeriod = bankPartnerConnections.Where(connection =>
				connection.Timestamp <= DateTimeOffsetServer.Now.Subtract(_bankPartnerConnectionsConfig.GracePeriod))
			.ToList();

		if (connectionsPastOrAtGracePeriod.Any())
		{
			return connectionsPastOrAtGracePeriod
				.OrderByDescending(connection => connection.Timestamp)
				.First();
		}

		return bankPartnerConnections
			.OrderBy(connection => connection.Timestamp)
			.First();
	}
}
