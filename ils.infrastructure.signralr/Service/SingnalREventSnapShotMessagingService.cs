using core.Domain.Entities;
using ils.infrastructure.Messaging;

namespace ils.infrastructure.signalr.Service
{
    internal class SingnalREventSnapShotMessagingService : IEventSnapShotMessagingService
    {
        public Task SendEventSnapshotsAsync(IEnumerable<BatchEvent> snapshots)
        {
            throw new NotImplementedException();
        }
    }
}
