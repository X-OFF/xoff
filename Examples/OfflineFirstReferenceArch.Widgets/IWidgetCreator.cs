using System.Threading.Tasks;
using OfflineFirstReferenceArch.Models;

namespace OfflineFirstReferenceArch.Widgets
{
    public interface IWidgetCreator
    {
        Task<XOFFOperationResult> SeedIfEmpty();
        XOFFOperationResult Create(Widget widget);

    }

    public interface IWidgetDeleter
    {
        XOFFOperationResult Delete(Widget widget);
    }
}