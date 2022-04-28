using Azure.Data.Tables;

namespace RTGS.Service.Storage;

public interface IStorageTableResolver
{
	TableClient GetTable(string tableName);
}
