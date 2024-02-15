using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using System.Linq.Expressions;

namespace ils.infrastructure.azureTableStorage.Client
{
    internal interface ITableClient
    {
        Task AddEntityAsync<T>(T entity) where T : class, ITableEntity;
        IAsyncEnumerable<T> QueryAsync<T>(Expression<Func<T, bool>> value) where T: class,ITableEntity;
    }
}
