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
	    private readonly IWidgetDeleter _widgetDeleter;

	    public LandingPageViewModel(IWidgetReader widgetGetter, IWidgetCreator widgetCreator, IWidgetDeleter widgetDeleter)
		{
			_widgetGetter = widgetGetter;
			_widgetCreator = widgetCreator;
		    _widgetDeleter = widgetDeleter;
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
            widget.LocalId = Guid.NewGuid();

		    var result = _widgetCreator.Create(widget);
			if (result.Success) 
			{
				Widgets.Add(widget);
			}
		}

	    public XOFFOperationResult Delete(Widget widget)
	    {
	        return _widgetDeleter.Delete(widget);
	    }
	}
}
