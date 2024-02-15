using System.Threading.Tasks;
using ils.core.Domain.Entities;
using ils.infrastructure.azureTableStorage.Client;
using ils.infrastructure.azureTableStorage.DataEntity;

namespace ils.infrastructure.DataAccessor
{
    internal class BatchEventDataAccessor : TableStorageDataAccessorBase, IBatchEventDataAccessor
    {
        public BatchEventDataAccessor(IDatabaseClient azureTableStorageClient, string tableName) : base(azureTableStorageClient, tableName)
        {
        }

        public async Task AddAsync(BatchEvent e)
        {
            await _tableClient.AddEntityAsync(e.ToTableEntity());
        }
    }
}




