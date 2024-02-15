
using ils.Repository;
using ils.core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ils.Apps.concrete
{
    internal class EventService : IEventService
    {
        private readonly EventRepository _eventRepository;
        private readonly EventSnapshotRepository _eventSnapshotRepository;
        private int _baseTime;
        public EventService(EventRepository eventRepository, EventSnapshotRepository eventSnapshotRepository, int baseTime = 0)
        {
            _eventRepository = eventRepository;
            _eventSnapshotRepository = eventSnapshotRepository;
            _baseTime = baseTime;
        }

        // イベントを処理する
        public async Task<IEnumerable<BatchEvent>> AddEventAsync(BatchEvent e)
        {
            await _eventRepository.SaveEventAsync(e);

            return await GetEventSnapshotsAsync();
        }

        // 指定時間のバッチ状況を取得する
        public async Task<IEnumerable<BatchEvent>> GetEventSnapshotsAsync(DateTime? targetDate = null)
        {
            return await _eventSnapshotRepository.GetEventSnapshotListAsync(targetDate, _baseTime);
        }

    }
}