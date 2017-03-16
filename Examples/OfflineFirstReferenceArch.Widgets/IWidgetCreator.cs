using OfflineFirstReferenceArch.Models;

namespace OfflineFirstReferenceArch.Widgets
{
    public interface IWidgetCreator
    {
        OperationResult SeedIfEmpty();
        OperationResult Create(Widget widget);

    }
}