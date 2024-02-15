using Azure.Data.Tables;

namespace ils.infrastructure.DataAccessor 
{
    internal abstract class TableStorageDataAccessorBase
    {

        protected readonly TableServiceClient _azureTableStorageClient;
        protected readonly TableClient _tableClient;

        protected TableStorageDataAccessorBase(TableServiceClient azureTableStorageClient, string tableName)
        {
            _azureTableStorageClient = azureTableStorageClient;
            _tableClient = _azureTableStorageClient.GetTableClient(tableName);
        }
    }

}

