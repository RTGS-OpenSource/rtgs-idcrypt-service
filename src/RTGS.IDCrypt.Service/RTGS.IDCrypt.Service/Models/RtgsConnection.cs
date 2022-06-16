using Azure;
using Azure.Data.Tables;

namespace RTGS.IDCrypt.Service.Models;

public class RtgsConnection : ITableEntity
{
	public string PartitionKey { get; set; }
	public string RowKey { get; set; }
	public string Alias { get; set; }
	public string ConnectionId { get; set; }
	public string Status { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? ActivatedAt { get; set; }
	public DateTimeOffset? Timestamp { get; set; }
	public ETag ETag { get; set; }
}
