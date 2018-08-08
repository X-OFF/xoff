using System;
using OfflineFirstReferenceArch.Models;
using XOFF.Core.Repositories;

namespace OfflineFirstReferenceArch.Widgets
{
    public class WidgetDeleter : IWidgetDeleter
    {
        private readonly ISyncedRepository<Widget, Guid> _repository;
        public WidgetDeleter(ISyncedRepository<Widget, Guid> repository)
        {
            _repository = repository;
        }
        public XOFFOperationResult Delete(Widget widget)
        {
            try
            {
                _repository.Delete(widget.LocalId);
            }
            catch (Exception ex)
            {
                return XOFFOperationResult.CreateFailure(ex);
            }
            return XOFFOperationResult.CreateSuccessResult("Success");
        }
    }
}