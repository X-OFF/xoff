using System;

namespace XOFF.Core
{
    public interface IModel<TIdentifier>  
    {
        TIdentifier Id { get; set; } //todo make this generic if possible 

        /*
             todo: these really should be datetimeoffsets which 
             probably means that these need to have string 
             backing stores and not map the date       
         */
        DateTime LastTimeSynced { get; set; }
    }
}