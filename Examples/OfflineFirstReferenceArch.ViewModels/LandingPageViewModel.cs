using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			await _widgetCreator.SeedIfEmpty();
            Widgets = new ObservableCollection<Widget>(await _widgetGetter.GetAll());
        }
        
        public ObservableCollection<Widget> Widgets { get; set; }
    }
}
