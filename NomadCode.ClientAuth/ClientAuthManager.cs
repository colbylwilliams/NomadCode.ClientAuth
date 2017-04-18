using System;

#if __IOS__

using UIKit;
using Foundation;

#if NC_AUTH_GOOGLE
using Google.SignIn;
#endif
#if NC_AUTH_FACEBOOK
using Facebook.CoreKit;
#endif
#if NC_AUTH_MICROSOFT
#endif
#if NC_AUTH_TWITTER
#endif

#endif

namespace NomadCode.ClientAuth
{
	public partial class ClientAuthManager
	{
		static string KeychainServiceName (ClientAuthProviders provider, ClientAuthDetailTypes type)
		{
			return $"{provider.ToString ().ToLower ()}.{type.ToString ().ToLower ()}";
		}

		static ClientAuthManager _shared;
		public static ClientAuthManager Shared => _shared ?? (_shared = new ClientAuthManager ());


		ClientAuthManager () { }


		public event EventHandler<ClientAuthDetails> AthorizationChanged;


		ClientAuthDetails _clientAuthDetails;
		public ClientAuthDetails ClientAuthDetails
		{
			get => _clientAuthDetails ?? (_clientAuthDetails = initClientAuthDetails () ?? null);
			private set
			{
				ClientAuthProviders? provider = _clientAuthDetails?.ClientAuthProvider;

				_clientAuthDetails = value;

				if (_clientAuthDetails == null)
				{
					if (provider.HasValue)
					{
						removeItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.Name));
						removeItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.Username));
						removeItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.Email));
						removeItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.AuthCode));
						removeItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.AvatarUrl));
					}
				}
				else
				{
					if (!string.IsNullOrEmpty (_clientAuthDetails.Token)) saveItemToKeychain (KeychainServiceName (_clientAuthDetails.ClientAuthProvider, ClientAuthDetailTypes.Token), ClientAuthDetailTypes.Token.ToString (), _clientAuthDetails.Token);
					if (!string.IsNullOrEmpty (_clientAuthDetails.Name)) saveItemToKeychain (KeychainServiceName (_clientAuthDetails.ClientAuthProvider, ClientAuthDetailTypes.Name), ClientAuthDetailTypes.Name.ToString (), _clientAuthDetails.Name);
					if (!string.IsNullOrEmpty (_clientAuthDetails.Username)) saveItemToKeychain (KeychainServiceName (_clientAuthDetails.ClientAuthProvider, ClientAuthDetailTypes.Username), ClientAuthDetailTypes.Username.ToString (), _clientAuthDetails.Username);
					if (!string.IsNullOrEmpty (_clientAuthDetails.Email)) saveItemToKeychain (KeychainServiceName (_clientAuthDetails.ClientAuthProvider, ClientAuthDetailTypes.Email), ClientAuthDetailTypes.Email.ToString (), _clientAuthDetails.Email);
					if (!string.IsNullOrEmpty (_clientAuthDetails.AuthCode)) saveItemToKeychain (KeychainServiceName (_clientAuthDetails.ClientAuthProvider, ClientAuthDetailTypes.AuthCode), ClientAuthDetailTypes.AuthCode.ToString (), _clientAuthDetails.AuthCode);
					if (!string.IsNullOrEmpty (_clientAuthDetails.AvatarUrl)) saveItemToKeychain (KeychainServiceName (_clientAuthDetails.ClientAuthProvider, ClientAuthDetailTypes.AvatarUrl), ClientAuthDetailTypes.AvatarUrl.ToString (), _clientAuthDetails.AvatarUrl);
				}
			}
		}


		public void SetClientAuthDetails (ClientAuthDetails details)
		{
			ClientAuthDetails = details;

			AthorizationChanged?.Invoke (this, ClientAuthDetails);
		}


#if __IOS__

		public void InitializeAuthProviders (UIApplication application, NSDictionary launchOptions)
		{
#if NC_AUTH_GOOGLE
			var googleServiceDictionary = NSDictionary.FromFile ("GoogleService-Info.plist");

			if (googleServiceDictionary == null)
			{
				throw new System.IO.FileNotFoundException ("Must be present to use Google Auth", "GoogleService-Info.plist");
			}

			SignIn.SharedInstance.ClientID = googleServiceDictionary ["CLIENT_ID"].ToString ();
			SignIn.SharedInstance.ServerClientID = googleServiceDictionary ["SERVER_CLIENT_ID"].ToString ();
#endif
#if NC_AUTH_FACEBOOK
			ApplicationDelegate.SharedInstance.FinishedLaunching (application, launchOptions);
#endif
		}


		public bool OpenUrl (UIApplication app, NSUrl url, NSDictionary options)
		{
			var openUrlOptions = new UIApplicationOpenUrlOptions (options);

			var opened = false;
#if NC_AUTH_GOOGLE
			opened = SignIn.SharedInstance.HandleUrl (url, openUrlOptions.SourceApplication, openUrlOptions.Annotation);

#endif
#if NC_AUTH_FACEBOOK
			if (opened)
			{
				opened = ApplicationDelegate.SharedInstance.OpenUrl (app, url, openUrlOptions.SourceApplication, openUrlOptions.Annotation);
			}
#endif
			return opened;

			//return base.OpenUrl(app, url, options);
		}

#elif __ANDROID__

		public void InitializeAuthProviders ()
		{
			//var webClientId = context.GetString (Resource.String.default_web_client_id);

			//Android.Gms.Auth.Api.SignIn.GoogleSignInOptions gso = new Android.Gms.Auth.Api.SignIn.GoogleSignInOptions.Builder (Android.Gms.Auth.Api.SignIn.GoogleSignInOptions.DefaultSignIn)
			//												 .RequestEmail ()
			//												 .RequestIdToken (webClientId)
			//												 .RequestServerAuthCode (webClientId)
			//												 .Build ();

			//googleApiClient = new Android.Gms.Common.Apis.GoogleApiClient.Builder (context)
			//.EnableAutoManage (context, async (Android.Gms.Common.ConnectionResult obj) =>
			//{

			//})
			//.AddApi (Android.Gms.Auth.Api.Auth.GOOGLE_SIGN_IN_API, gso)
			//.Build ();
		}
#endif


		ClientAuthDetails initClientAuthDetails ()
		{
			var providers = new [] { ClientAuthProviders.Google, ClientAuthProviders.Facebook, ClientAuthProviders.Microsoft, ClientAuthProviders.Twitter };

			var clientAuthDetails = new ClientAuthDetails ();

			ClientAuthProviders? provider = null;

			string token = null;// = getItemFromKeychain (KeychainServiceName (ClientAuthProviders.Google, ClientAuthDetailTypes.Token));

			foreach (var item in providers)
			{
				token = getItemFromKeychain (KeychainServiceName (item, ClientAuthDetailTypes.Token)).PrivateKey;

				if (!string.IsNullOrEmpty (token))
				{
					provider = item;

					break;
				}
			}

			if (provider.HasValue)
			{
				Log.Debug ($"Existing ClientAuthProvider found: {provider.Value}");

				clientAuthDetails.ClientAuthProvider = provider.Value;
				clientAuthDetails.Token = token;
				clientAuthDetails.Name = getItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.Name)).PrivateKey;
				clientAuthDetails.Username = getItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.Username)).PrivateKey;
				clientAuthDetails.Email = getItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.Email)).PrivateKey;
				clientAuthDetails.AuthCode = getItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.AuthCode)).PrivateKey;
				clientAuthDetails.AvatarUrl = getItemFromKeychain (KeychainServiceName (provider.Value, ClientAuthDetailTypes.AvatarUrl)).PrivateKey;

				return clientAuthDetails;
			}

			return null;
		}
	}
}
