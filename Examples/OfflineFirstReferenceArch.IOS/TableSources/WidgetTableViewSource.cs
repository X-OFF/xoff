using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using OfflineFirstReferenceArch.Models;
using UIKit;

namespace OfflineFirstReferenceArch.IOS.TableSources
{

	public class WidgetTableViewSource : UITableViewSource
	{
		private IList<Widget> _widgets;
	    private readonly UITableView _tableView;

	    public EventHandler<KeyValuePair<NSIndexPath,Widget>> RowDeleted;
	    private WeakReference<UITableView> _tableViewReference;

	    public WidgetTableViewSource(IList<Widget> widgets, UITableView tableView)
        {
            _widgets = widgets;
            _tableViewReference = new WeakReference<UITableView>(tableView);
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
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true; // return false if you wish to disable editing for a specific indexPath or for all rows
        }
        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableViewCellEditingStyle.Delete; // this example doesn't suppport Insert
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                
                    var widget = _widgets.ElementAt(indexPath.Row);
                    RowDeleted?.Invoke(this,new KeyValuePair<NSIndexPath,Widget>(indexPath,widget));
                    break;
            }
        }

	    public void RemoveItem(NSIndexPath indexPath)
	    {
	        _widgets.RemoveAt(indexPath.Row);
	        UITableView tableview;
	        _tableViewReference.TryGetTarget(out tableview);
	        tableview?.DeleteRows(new NSIndexPath[] {indexPath}, UITableViewRowAnimation.Fade);
	    }
	}
}
