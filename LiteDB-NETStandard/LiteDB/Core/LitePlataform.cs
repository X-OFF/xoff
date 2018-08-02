using System;

namespace LiteDB
{
    [Android.Runtime.Preserve(AllMembers = true)]
    public class LitePlatform
    {
        [Android.Runtime.Preserve]
        public static ILitePlatform Platform;

#if NETFULL
        static LitePlatform()
        {
            LitePlatform.Platform = new LiteDB.Platform.LitePlatformFullDotNet();
        }
#endif

        public static void ThrowIfNotInitialized()
        {
            if (Platform == null)
            {
                throw new PlatformNotInitializedException();
            }
        }

        [Android.Runtime.Preserve]
        public static void Initialize(ILitePlatform platform)
        {
            Platform = platform;
        }

#if NETSTANDARD
        public static void InitializeDefault(String path = ".", bool useAsyncFileHandler = false)
        {
            Initialize(new Platform.LitePlatformDotNetStandardDefault(path, useAsyncFileHandler));
        }
#endif
    }
}
