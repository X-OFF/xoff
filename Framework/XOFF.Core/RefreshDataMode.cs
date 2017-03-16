namespace XOFF.Core.Settings
{
    public enum RefreshDataMode
    {
        /// <summary>
        /// Go to the local repository first, go to the server if the data is "Stale"
        /// </summary>
        IfStale = 0,
        /// <summary>
        /// Always go to the server first refresh the data locally then return it.
        /// </summary>
        IfOnline = 1,
        /// <summary>
        /// Only Refresh the data by the Refresh(); method. 
        /// </summary>
        OnlyOnRefresh = 2
    }
}