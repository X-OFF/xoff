namespace XOFF.Core.Repositories.Settings
{
    public class SyncRepositorySettings
    {
		public SyncRepositorySettings(int refreshSeconds = 86400, RefreshDataMode refreshDataMode = RefreshDataMode.RefreshIfStale)//default to 1 day
        {
            RefreshSecondsThreshold = refreshSeconds;
			RefreshDataMode = refreshDataMode;
        }

		public RefreshDataMode RefreshDataMode { get; private set; }
        public int RefreshSecondsThreshold { get; }

    }
}