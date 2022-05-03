using Azure.Data.Tables;

namespace RTGS.IDCrypt.Service.Storage;

public interface IStorageTableResolver
{
	TableClient GetTable(string tableName);
}
