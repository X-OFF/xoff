using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using OfflineFirstReferenceArch.Models;
using OfflineFirstReferenceArch.Widgets;

namespace OfflineFirstReferenceArch.ViewModels
{
	public class LandingPageViewModel
	{
		private readonly IWidgetReader _widgetGetter;
		private readonly IWidgetCreator _widgetCreator;

		public LandingPageViewModel(IWidgetReader widgetGetter, IWidgetCreator widgetCreator)
		{
			_widgetGetter = widgetGetter;
			_widgetCreator = widgetCreator;

		}
		public async Task Initialize()
		{
			var widgets = await _widgetGetter.GetAll();
			Widgets = new ObservableCollection<Widget>(widgets);
		}

        public ObservableCollection<Widget> Widgets { get; set; }

		public void CreateNewWidget(string widgetName)
		{
			var widget = new Widget();

			widget.Name = widgetName;
		    widget.Id = Guid.NewGuid();

		    var result = _widgetCreator.Create(widget);
			if (result.Success) 
			{
				Widgets.Add(widget);
			}
		}
	}
}
