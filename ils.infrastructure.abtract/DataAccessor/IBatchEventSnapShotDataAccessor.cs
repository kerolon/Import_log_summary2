using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ils.core.Domain.Entities;

namespace ils.infrastructure.DataAccessor 
{
    public interface IBatchEventSnapShotDataAccessor
    {
        Task<IEnumerable<BatchEvent>> GetAsync(DateTime? targetDate = null, int baseTime = 0);
    }
}


