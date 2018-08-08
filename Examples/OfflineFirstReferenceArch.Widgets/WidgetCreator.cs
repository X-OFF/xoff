using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<XOFFOperationResult> SeedIfEmpty()
        {
            try
            {
				var getWidgetsResult = await _repository.Get();
                if (!getWidgetsResult.Success || !getWidgetsResult.Result.Any())
				{
                    var widgets = new List<Widget>();
                    for (int i = 0; i < 15; i++)
                    {
                        var guid = Guid.NewGuid();
                        widgets.Add(new Widget()
                        {
                            LocalId = guid,
                            Name = guid.ToString()

                        });
                    }
					_repository.Insert(widgets);
                }
                return XOFFOperationResult.CreateSuccessResult("There are widgets saved");
            }
            catch (Exception e)
            {
				return XOFFOperationResult.CreateFailure(e.Message);
            }
        }

        public XOFFOperationResult Create(Widget widget)
        {
            try
            {
				_repository.Insert(widget);
            }
            catch (Exception ex)
            {
                return XOFFOperationResult.CreateFailure(ex);
            }
            return XOFFOperationResult.CreateSuccessResult("Success");
        }
    }
}