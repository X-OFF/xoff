namespace XOFF.Core.Settings
{
    public class SyncRepositorySettings
    {
        public SyncRepositorySettings(int refreshSeconds = 86400, RefreshDataMode refreshDataMode = Settings.RefreshDataMode.IfStale)//default to 1 day
        {
            RefreshSecondsThreshold = refreshSeconds;
            RefreshDataMode = refreshDataMode;
        }

        public RefreshDataMode RefreshDataMode { get; }
        public int RefreshSecondsThreshold { get; }

    }
}