using System;
namespace XOFF.Core.ChangeQueue
{      
    public class ChangeQueueSettings
    {
        public ChangeQueueSettings(int failedAttemptLimit = 3)
        {
            FailedAttemptLimit = failedAttemptLimit;
        }

        public int FailedAttemptLimit { get; set; }
    }
}