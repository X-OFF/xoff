using XOFF.Core;
using XOFF.Core.Repositories;

namespace XOFF.SQLite
{
    public interface ISQLiteRepository<TModel,TIdentifier> : IRepository<TModel, TIdentifier> where TModel : class, IModel<TIdentifier>
    {

    }
}