#if __ANDROID__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;

using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Android.Support.V4.App;

#if NC_AUTH_GOOGLE
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
#endif


namespace NomadCode.ClientAuth
{
	[Activity (Label = "AuthActivity")]
	public class AuthActivity : FragmentActivity, View.IOnClickListener
#if NC_AUTH_GOOGLE
		, GoogleApiClient.IOnConnectionFailedListener//AppCompatActivity
#endif
	{
		const int RC_SIGN_IN = 9001;

#if NC_AUTH_GOOGLE
		GoogleApiClient googleApiClient;
#endif
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);


			// use this format to help users set up the app
			//if (GetString (Resource.String.google_app_id) == "YOUR-APP-ID")
			//throw new System.Exception ("Invalid google-services.json file.  Make sure you've downloaded your own config file and added it to your app project with the 'GoogleServicesJson' build action.");
#if NC_AUTH_GOOGLE
			var webClientId = GetString (Resource.String.default_web_client_id);

			GoogleSignInOptions gso = new GoogleSignInOptions.Builder (GoogleSignInOptions.DefaultSignIn)
															 .RequestEmail ()
															 .RequestIdToken (webClientId)
															 .RequestServerAuthCode (webClientId)
															 .Build ();

			googleApiClient = new GoogleApiClient.Builder (this)
												 .EnableAutoManage (this, this)
												 .AddApi (Auth.GOOGLE_SIGN_IN_API, gso)
												 .Build ();

			FindViewById<SignInButton> (Resource.Id.sign_in_button).SetOnClickListener (this);
#endif
		}


		public void OnClick (View v)
		{
			switch (v.Id)
			{
#if NC_AUTH_GOOGLE
				case Resource.Id.sign_in_button:
					signIn ();
					break;
#endif
				default:
					break;
			}
		}


		void signIn ()
		{
#if NC_AUTH_GOOGLE
			var signInIntent = Auth.GoogleSignInApi.GetSignInIntent (googleApiClient);

			StartActivityForResult (signInIntent, RC_SIGN_IN);
#endif
		}


		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (requestCode == RC_SIGN_IN)
			{
#if NC_AUTH_GOOGLE
				var result = Auth.GoogleSignInApi.GetSignInResultFromIntent (data);

				handleSignInResult (result);
#endif
			}
		}


#if NC_AUTH_GOOGLE
		void handleSignInResult (GoogleSignInResult result)
		{
			//Log.d (TAG, "handleSignInResult:" + result.isSuccess ());
			if (result.IsSuccess)
			{
				// Signed in successfully, show authenticated UI.
				GoogleSignInAccount user = result.SignInAccount;

				if (user != null)
				{
					Log.Debug ($"user.Account.Name: {user.Account.Name}");
					Log.Debug ($"acct.DisplayName: {user.DisplayName}");
					Log.Debug ($"acct.Email: {user.Email}");
					Log.Debug ($"acct.FamilyName: {user.FamilyName}");
					Log.Debug ($"acct.GivenName: {user.GivenName}");
					Log.Debug ($"acct.GrantedScopes: {string.Join (",", user.GrantedScopes)}");
					Log.Debug ($"acct.Id: {user.Id}");
					Log.Debug ($"acct.IdToken: {user.IdToken}");
					Log.Debug ($"acct.PhotoUrl: {user.PhotoUrl}");
					Log.Debug ($"acct.ServerAuthCode: {user.ServerAuthCode}");

					var details = new ClientAuthDetails
					{
						ClientAuthProvider = ClientAuthProviders.Google,
						Username = user.DisplayName,
						Email = user.Email,
						Token = user.IdToken,
						AuthCode = user.ServerAuthCode,
						AvatarUrl = user.PhotoUrl.ToString ()
					};

					ClientAuthManager.Shared.SetClientAuthDetails (details);

					Finish ();
				}
			}
			else
			{
				// Signed out, show unauthenticated UI.
				Log.Error ($"Google SingIn failed with code:{result.Status}");
			}
		}


		public void OnConnectionFailed (ConnectionResult result)
		{
			Log.Error ($"{result.ErrorMessage} code: {result.ErrorCode}");
		}
#endif
	}
}

#endif