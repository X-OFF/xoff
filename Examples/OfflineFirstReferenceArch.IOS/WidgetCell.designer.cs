// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace OfflineFirstReferenceArch.IOS
{
	[Register ("WidgetCell")]
	partial class WidgetCell
	{
		[Outlet]
		UIKit.UILabel widgetNameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (widgetNameLabel != null) {
				widgetNameLabel.Dispose ();
				widgetNameLabel = null;
			}
		}
	}
}
