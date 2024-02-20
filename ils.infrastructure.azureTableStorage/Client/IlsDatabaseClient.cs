using Azure.Data.Tables;

namespace ils.infrastructure.azureTableStorage.Client
{
    internal class IlsDatabaseClient : IDatabaseClient
    {
        private readonly TableServiceClient _tableServiceClient;
        public IlsDatabaseClient(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;   
        }
        public ITableClient GetTableClient(string tableName)
        {
            return new IlsTableClient(_tableServiceClient.GetTableClient(tableName));
        }
    }
}
