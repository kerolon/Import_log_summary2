using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ils.core.Domain.Entities;
using ils.infrastructure.DataAccessor;
namespace ils.Repository
{
    internal class EventSnapshotRepository
    {
        private readonly IBatchEventSnapShotDataAccessor _batchEventSnapshotAccessor;

        public EventSnapshotRepository(IBatchEventSnapShotDataAccessor accessor)
        {
            _batchEventSnapshotAccessor = accessor;
        }

        // カテゴリ別にイベントの集約結果を取得する
        public async Task<IEnumerable<BatchEvent>> GetEventSnapshotListAsync(DateTime? targetDate = null, int baseTime = 0)
        {
            // 空のリストを作成する
            return await _batchEventSnapshotAccessor.GetAsync(targetDate, baseTime);
        }
    }
}

