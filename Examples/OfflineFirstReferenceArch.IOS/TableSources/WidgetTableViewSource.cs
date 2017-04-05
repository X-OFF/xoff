using System;
using System.Collections.Generic;
using Foundation;
using OfflineFirstReferenceArch.IOS;
using OfflineFirstReferenceArch.Models;
using UIKit;

namespace MyBankerPrototype.iOS
{

	public class WidgetTableViewSource : UITableViewSource
	{
		private IList<Widget> _widgets;

		public WidgetTableViewSource(IList<Widget> widgets)
		{
			_widgets = widgets;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = (WidgetCell)tableView.DequeueReusableCell(WidgetCell.Key, indexPath);
			cell.ConfigureCell(_widgets[indexPath.Row]);
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			return cell;
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			return 60;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			if (_widgets == null)
			{
				return 0;
			}
			return _widgets.Count;
		}

		public void SetWidgets(IList<Widget> widgets)
		{
			_widgets = widgets;
		}

		public void AddWidgets(IList<Widget> widgets)
		{
			foreach (var widget in widgets) 
			{
				_widgets.Add(widget);
			}
		}
	}
}
