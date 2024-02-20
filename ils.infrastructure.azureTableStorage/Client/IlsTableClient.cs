using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace ils.infrastructure.azureTableStorage.Client
{
    internal class IlsTableClient : ITableClient
    {
        private readonly TableClient _tableClient;
        public IlsTableClient(TableClient tableClient)
        {
            _tableClient = tableClient;
        }
        async Task ITableClient.AddEntityAsync<T>(T entity)
        {
            await _tableClient.AddEntityAsync(entity);
        }

        IAsyncEnumerable<T> ITableClient.QueryAsync<T>(Expression<Func<T, bool>> value)
        {
            return _tableClient.QueryAsync(value);
        }
    }
}
