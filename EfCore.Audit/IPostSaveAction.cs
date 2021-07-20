using System.Collections.Generic;
using System.Threading.Tasks;

namespace EfCore.Audit
{
    public interface IPostSaveAction<in TDbContext>
    {
        public Task Handle(TDbContext context, IList<Audit> changes);
    }
}