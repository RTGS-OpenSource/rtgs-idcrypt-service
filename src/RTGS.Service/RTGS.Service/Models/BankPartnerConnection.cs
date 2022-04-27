using Azure;
using Azure.Data.Tables;

namespace RTGS.Service.Models;

public class BankPartnerConnection : ITableEntity
{
	public string RtgsGlobalId { get; init; }
	public string Alias { get; init; }
	public string PublicDid { get; init; }
	public string ConnectionId { get; init; }
	public string PresentationExchangeId { get; init; }

	public string PartitionKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	public string RowKey { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	public DateTimeOffset? Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
	public ETag ETag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
