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
        public OperationResult Delete(Widget widget)
        {
            try
            {
                _repository.Delete(widget.Id);
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(ex);
            }
            return OperationResult.CreateSuccessResult("Success");
        }
    }
}