using System;

namespace XOFF.Core
{
    public interface IModel<TIdentifier>  
    {
        TIdentifier LocalId { get; set; } 

        /*
             todo: these really should be datetimeoffsets which 
             probably means that these need to have string 
             backing stores and not map the date       
         */
        DateTime LastTimeSynced { get; set; }

        string RemoteId { get; set; }

        int ApiSortOrder { get; set; }
    }
}