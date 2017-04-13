using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Autofac;
using MyBankerPrototype.iOS;
using OfflineFirstReferenceArch.Models;
using OfflineFirstReferenceArch.ViewModels;
using UIKit;
using System.Linq;
namespace OfflineFirstReferenceArch.IOS
{
	public partial class LandingPageViewController : UIViewController
	{
		

		private LandingPageViewModel _viewModel;

		WidgetTableViewSource _source;

		public LandingPageViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
	
			var addWidgetButton = new UIBarButtonItem("Add", UIBarButtonItemStyle.Plain, (sender, args) =>
				{
				var textInputAlertController = UIAlertController.Create("Create A widget", "Widget Name", UIAlertControllerStyle.Alert);

				//Add Text Input
				textInputAlertController.AddTextField(textField =>
				{
				});

				//Add Actions
				var cancelAction = UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, alertAction => Console.WriteLine("Cancel was Pressed"));
				var okayAction = UIAlertAction.Create("Okay", UIAlertActionStyle.Default, alertAction => CreateWidget(textInputAlertController));

					textInputAlertController.AddAction(cancelAction);
					textInputAlertController.AddAction(okayAction);

					//Present Alert
					PresentViewController(textInputAlertController, true, null);
				});

			this.NavigationItem.SetRightBarButtonItem(addWidgetButton, true);

            var refreshButton = new UIBarButtonItem("Add", UIBarButtonItemStyle.Plain, async (sender, args) =>
            {
                await _viewModel.Initialize();
                var list = new List<Widget>(_viewModel.Widgets);
                _source = new WidgetTableViewSource(list);
                widgetsTableView.Source = _source;
                widgetsTableView.ReloadData();
            });

            this.NavigationItem.SetLeftBarButtonItem(refreshButton, true);

            _viewModel = DIContainer.ContainerInstance.Resolve<LandingPageViewModel>();
			_viewModel.Initialize().ContinueWith((arg) =>
			{
				InvokeOnMainThread(() =>
				{
					var list = new List<Widget>(_viewModel.Widgets);
					_source = new WidgetTableViewSource(list);
					widgetsTableView.Source = _source;
					widgetsTableView.ReloadData();
					_viewModel.Widgets.CollectionChanged += Widgets_CollectionChanged;

				});
			});
		}

		void Widgets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var newWidgets = new List<Widget>();

			foreach (Widget widget in e.NewItems) 
			{
				newWidgets.Add(widget);
			}
			_source.AddWidgets(newWidgets);

			//todo remove on remove

			widgetsTableView.ReloadData();
            
		}

		void CreateWidget(UIAlertController controller)
		{
			var widgetName = controller.TextFields[0].Text;
			_viewModel.CreateNewWidget(widgetName);
		}
}
}
