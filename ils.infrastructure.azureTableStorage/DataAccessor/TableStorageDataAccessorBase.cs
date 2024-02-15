using Azure.Data.Tables;
using ils.infrastructure.azureTableStorage.Client;

namespace ils.infrastructure.DataAccessor 
{
    internal abstract class TableStorageDataAccessorBase
    {

        protected readonly IDatabaseClient _azureTableStorageClient;
        protected readonly ITableClient _tableClient;

        protected TableStorageDataAccessorBase(IDatabaseClient azureTableStorageClient, string tableName)
        {
            _azureTableStorageClient = azureTableStorageClient;
            _tableClient = _azureTableStorageClient.GetTableClient(tableName);
        }
    }

}

