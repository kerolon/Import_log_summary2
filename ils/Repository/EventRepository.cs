using System.Threading.Tasks;
using ils.core.Domain.Entities;
using ils.infrastructure.DataAccessor;
namespace ils.Repository 
{
    internal class EventRepository
    {
        // Table Storageのクライアントを依存性注入する
        private readonly IBatchEventDataAccessor _batchEventAccessor;

        public EventRepository(IBatchEventDataAccessor batchEventAccessor)
        {
            _batchEventAccessor = batchEventAccessor;
        }

        // イベントを保存する
        public async Task SaveEventAsync(BatchEvent e)
        {
            await _batchEventAccessor.AddAsync(e);
        }
    }
}
