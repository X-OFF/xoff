using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OfflineFirstReferenceArch.Models;
using XOFF.Core.Repositories;

namespace OfflineFirstReferenceArch.Widgets
{
	public class WidgetReader : IWidgetReader
	{
		private readonly ISyncedRepository<Widget, Guid> _repository;

		public WidgetReader(ISyncedRepository<Widget, Guid> repository)
		{
			_repository = repository;
		}

		public async Task<IList<Widget>> GetAll()//should probably return the operation result
		{
			var result = await _repository.Get();
			if (result.Success)
			{
				return result.Result;
			}
			else
			{
				return new List<Widget>();
			}

		}

		public async Task<Widget> GetWidget(Guid id)
		{
			var widgetResult = await _repository.Get(id);
			return widgetResult.Success ? widgetResult.Result : null;
		}
	}
}
