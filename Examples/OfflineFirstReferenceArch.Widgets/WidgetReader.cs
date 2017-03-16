using System;
using System.Collections.Generic;
using System.Text;
using OfflineFirstReferenceArch.Models;
using XOFF.Core.Repositories;

namespace OfflineFirstReferenceArch.Widgets
{
    public class WidgetReader : IWidgetReader
    {
        private readonly ISyncedRepository<Widget,Guid> _repository;

        public WidgetReader(ISyncedRepository<Widget,Guid> repository)
        {
            _repository = repository;
        }

        public IList<Widget> GetAll()//should probably return the operation result
        {
            var result = _repository.Get();
            if (result.Success)
            {
                return result.Result;
            }
            else
            {
                return new List<Widget>();
            }

        }

        public Widget GetWidget(Guid id)
        {
            var widgetResult = _repository.Get(id);
            return widgetResult.Success ? widgetResult.Result : null;
        }
    }
}
