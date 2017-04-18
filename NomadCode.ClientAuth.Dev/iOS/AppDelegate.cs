using Foundation;
using UIKit;

namespace NomadCode.ClientAuth.Dev.iOS
{
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window { get; set; }


		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			ClientAuthManager.Shared.InitializeAuthProviders (application, launchOptions);

			return true;
		}


		public override bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
		{
			return ClientAuthManager.Shared.OpenUrl (app, url, options) || base.OpenUrl (app, url, options);
		}
	}
}

