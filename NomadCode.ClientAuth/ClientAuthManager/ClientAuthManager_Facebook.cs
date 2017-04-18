﻿#if __IOS__

using UIKit;
using Foundation;

#if NC_AUTH_FACEBOOK
using Facebook.CoreKit;
#endif

#elif __ANDROID__

using Android.Support.V4.App;
using Android.Content;

#if NC_AUTH_FACEBOOK
#endif

#endif
namespace NomadCode.ClientAuth
{
    public partial class ClientAuthManager
    {
#if __IOS__

#if NC_AUTH_GOOGLE

        void initializeAuthProviderFacebook (UIApplication application, NSDictionary launchOptions)
        {
            ApplicationDelegate.SharedInstance.FinishedLaunching (application, launchOptions);
        }

        bool openUrlFacebook (UIApplication app, NSUrl url, UIApplicationOpenUrlOptions openUrlOptions)
        {
            return ApplicationDelegate.SharedInstance.OpenUrl (app, url, openUrlOptions.SourceApplication, openUrlOptions.Annotation);
        }

#else
        void initializeAuthProviderFacebook ();

        bool openUrlFacebook (UIApplication app, NSUrl url, UIApplicationOpenUrlOptions openUrlOptions) => false;
#endif


#elif __ANDROID__

#if NC_AUTH_GOOGLE

#else

#endif

#endif

    }
}
