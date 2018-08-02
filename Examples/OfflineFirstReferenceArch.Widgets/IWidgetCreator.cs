using System.Threading.Tasks;
using OfflineFirstReferenceArch.Models;

namespace OfflineFirstReferenceArch.Widgets
{
    public interface IWidgetCreator
    {
        Task<OperationResult> SeedIfEmpty();
        OperationResult Create(Widget widget);

    }

    public interface IWidgetDeleter
    {
        OperationResult Delete(Widget widget);
    }
}