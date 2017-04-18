using System;

using UIKit;

using Google.SignIn;

namespace NomadCode.ClientAuth.Dev.iOS
{
	public partial class ViewController : UIViewController
	{

		int count = 1;

		public ViewController (IntPtr handle) : base (handle) { }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();


			ClientAuthManager.Shared.AthorizationChanged += handleClientAuthChanged;


			// Perform any additional setup after loading the view, typically from a nib.
			Button.AccessibilityIdentifier = "myButton";
			Button.TouchUpInside += delegate
			{
				var title = string.Format ("{0} clicks!", count++);
				Button.SetTitle (title, UIControlState.Normal);
			};
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			login ();
		}


		void handleClientAuthChanged (object s, ClientAuthDetails e)
		{
			Log.Debug ($"Authenticated: {e}");
		}


		void login ()
		{
			try
			{
				var details = ClientAuthManager.Shared.ClientAuthDetails;

				if (details != null)
				{
					System.Diagnostics.Debug.WriteLine ($"ClientAuthDetails ClientAuthProvider: {details.ClientAuthProvider}");
					System.Diagnostics.Debug.WriteLine ($"ClientAuthDetails               Name: {details.Name}");
					System.Diagnostics.Debug.WriteLine ($"ClientAuthDetails           Username: {details.Username}");
					System.Diagnostics.Debug.WriteLine ($"ClientAuthDetails              Email: {details.Email}");
					System.Diagnostics.Debug.WriteLine ($"ClientAuthDetails              Token: {details.Token}");
					System.Diagnostics.Debug.WriteLine ($"ClientAuthDetails           AuthCode: {details.AuthCode}");
					System.Diagnostics.Debug.WriteLine ($"ClientAuthDetails          AvatarUrl: {details.AvatarUrl}");
				}

				else // otherwise prompt the user to login
				{
					BeginInvokeOnMainThread (() =>
					{
						var authViewController = new AuthViewController ();

						if (authViewController != null)
						{
							var authNavController = new UINavigationController (authViewController);

							if (authNavController != null)
							{
								PresentViewController (authNavController, true, null);
							}
						}
					});
				}
			}
			catch (Exception ex)
			{
				Log.Error (ex.Message);
				throw;
			}
		}


		void logout ()
		{
			try
			{
				SignIn.SharedInstance.SignOutUser ();

				login ();
			}
			catch (Exception ex)
			{
				Log.Error (ex.Message);
				throw;
			}
		}
	}
}
