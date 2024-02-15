using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ils.core.Domain.Entities;

namespace ils.Apps
{
    public interface IEventService
    {
        Task<IEnumerable<BatchEvent>> AddEventAsync(BatchEvent e);
        Task<IEnumerable<BatchEvent>> GetEventSnapshotsAsync(DateTime? targetDate = null);
    }
}
