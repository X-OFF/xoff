using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfflineFirstReferenceArch.Models;

namespace OfflineFirstReferenceArch.Widgets
{
    public interface IWidgetReader
    {
        Task<IList<Widget>> GetAll();
        Task<Widget> GetWidget(Guid name);
    }
}