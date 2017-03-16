using System;
using System.Collections.Generic;
using OfflineFirstReferenceArch.Models;

namespace OfflineFirstReferenceArch.Widgets
{
    public interface IWidgetReader
    {
        IList<Widget> GetAll();
        Widget GetWidget(Guid name);
    }
}