using System;
using System.Collections.Generic;
using System.Linq;
using OfflineFirstReferenceArch.Models;
using XOFF.Core.Repositories;

namespace OfflineFirstReferenceArch.Widgets
{
    public class WidgetCreator : IWidgetCreator
    {
        private readonly ISyncedRepository<Widget, Guid> _repository;

        public WidgetCreator(ISyncedRepository<Widget, Guid> repository)
        {
            _repository = repository;
        }

        public OperationResult SeedIfEmpty()
        {
            try
            {
                var getWidgetsResult = _repository.Get();
                if (!getWidgetsResult.Success || !getWidgetsResult.Result.Any())
                {
                    var widgets = new List<Widget>();
                    for (int i = 0; i < 15; i++)
                    {
                        var guid = Guid.NewGuid();
                        widgets.Add(new Widget()
                        {
                            Id = guid,
                            Name = guid.ToString()

                        });
                    }
                    _repository.Upsert(widgets);
                }
                return OperationResult.CreateSuccessResult("There are widgets saved");
            }
            catch (Exception e)
            {
				return OperationResult.CreateFailure(e.Message);
            }
        }

        public OperationResult Create(Widget widget)
        {
            try
            {
                _repository.Upsert(widget);
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(ex);
            }
            return OperationResult.CreateSuccessResult("Success");
        }
    }
}