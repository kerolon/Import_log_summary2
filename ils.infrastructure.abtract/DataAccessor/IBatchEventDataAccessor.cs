using System.Threading.Tasks;
using ils.core.Domain.Entities;

namespace ils.infrastructure.DataAccessor 
{
    public interface IBatchEventDataAccessor
    {
        Task AddAsync(BatchEvent e);
    }
}


