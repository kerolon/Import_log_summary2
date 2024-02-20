namespace ils.infrastructure.azureTableStorage.Client
{
    internal interface IDatabaseClient
    {
        ITableClient GetTableClient(string tableName);
    }
}
