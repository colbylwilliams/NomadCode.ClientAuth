﻿#if __IOS__ || __ANDROID__


#if __IOS__

using Foundation;
using Security;

#elif __ANDROID__

using System;
using System.Collections.Generic;

using Java.IO;
using Java.Security;
using Javax.Crypto;

using Android.Content;

#endif

namespace NomadCode.ClientAuth
{
#if CSHARP_6
    public class NamedTupleWorkaround
    {
        public string Account { get; set; }
        public string PrivateKey { get; set; }
    }
#endif

    public partial class ClientAuthManager
    {

#if __IOS__

        SecRecord genericRecord (string service) => new SecRecord (SecKind.GenericPassword)
        {
            Service = $"{NSBundle.MainBundle.BundleIdentifier}.nomadcode.clientauth-{service}"
        };


#if CSHARP_6
        NamedTupleWorkaround getItemFromKeychain (string service)
        {
            SecStatusCode status;

            var record = SecKeyChain.QueryAsRecord (genericRecord (service), out status);

            if (status == SecStatusCode.Success && record != null)
            {
                var account = record.Account;

                var privateKey = NSString.FromData (record.ValueData, NSStringEncoding.UTF8).ToString ();

                return new NamedTupleWorkaround { Account = account, PrivateKey = privateKey };
            }

            return new NamedTupleWorkaround();
        }
#else
        (string Account, string PrivateKey) getItemFromKeychain (string service)
        {
            var record = SecKeyChain.QueryAsRecord (genericRecord (service), out SecStatusCode status);

            if (status == SecStatusCode.Success && record != null)
            {
                var account = record.Account;

                var privateKey = NSString.FromData (record.ValueData, NSStringEncoding.UTF8).ToString ();

                return (account, privateKey);
            }

            return (null, null);
        }
#endif


        bool saveItemToKeychain (string service, string account, string privateKey)
        {
            var record = genericRecord (service);

            record.Account = account;

            record.ValueData = NSData.FromString (privateKey, NSStringEncoding.UTF8);

            // Delete any existing items
            SecKeyChain.Remove (record);

            // Add the new keychain item
            var status = SecKeyChain.Add (record);

            var success = status == SecStatusCode.Success;

            if (!success)
            {
                System.Diagnostics.Debug.WriteLine ($"Error in Keychain: {status}");
                System.Diagnostics.Debug.WriteLine ($"If you are seeing error code '-34018' got to Project Options -> iOS Bundle Signing -> make sure Entitlements.plist is populated for Custom Entitlements for iPhoneSimulator configs");
            }

            return success;
        }


        bool removeItemFromKeychain (string service)
        {
            var record = genericRecord (service);

            var status = SecKeyChain.Remove (record);

            var success = status == SecStatusCode.Success;

            if (!success)
            {
                System.Diagnostics.Debug.WriteLine ($"Error in Keychain: {status}");
                System.Diagnostics.Debug.WriteLine ($"If you are seeing error code '-34018' got to Project Options -> iOS Bundle Signing -> make sure Entitlements.plist is populated for Custom Entitlements for iPhoneSimulator configs");
            }

            return success;
        }

#else

        static Dictionary<string, KeyStore> keyStoresCache = new Dictionary<string, KeyStore> ();


        KeyStore getKeystore (string service)
        {
            var context = Android.App.Application.Context;


            var serviceId = $"{context.PackageName}.nomadcode.clientauth-{service}";

#if CSHARP_6
            KeyStore keystore;
            if (keyStoresCache.TryGetValue (serviceId, out keystore))
#else

        	if (keyStoresCache.TryGetValue (serviceId, out KeyStore keystore))
#endif

            {
                return keystore;
            }

            var password = service.ToCharArray ();

            keystore = KeyStore.GetInstance (KeyStore.DefaultType);

            // var protection = new KeyStore.PasswordProtection (password);

            try
            {
                // TODO: this isn't right, fix it
                using (var stream = context.OpenFileInput (serviceId))
                {
                    keystore.Load (stream, password);
                }
            }
            catch (FileNotFoundException)
            {
                keystore.Load (null, password);
            }

            keyStoresCache [serviceId] = keystore;

            return keystore;
        }


#if CSHARP_6
        NamedTupleWorkaround getItemFromKeychain (string service)
        {
            var context = Android.App.Application.Context;

            var password = service.ToCharArray ();

            var protection = new KeyStore.PasswordProtection (password);

            var keystore = getKeystore (service);

            var aliases = keystore.Aliases ();

            while (aliases.HasMoreElements)
            {
                var alias = aliases.NextElement ().ToString ();

                var item = keystore.GetEntry (alias, protection) as KeyStore.SecretKeyEntry;

                if (item != null)
                {
                    var bytes = item.SecretKey.GetEncoded ();

                    var serialized = System.Text.Encoding.UTF8.GetString (bytes);

                    return new NamedTupleWorkaround { Account = alias, PrivateKey = serialized };
                }
            }

            return new NamedTupleWorkaround ();
        }
#else
        (string Account, string PrivateKey) getItemFromKeychain (string service)
        {
            var context = Android.App.Application.Context;

            var password = service.ToCharArray ();

            var protection = new KeyStore.PasswordProtection (password);

            var keystore = getKeystore (service);

            var aliases = keystore.Aliases ();

            while (aliases.HasMoreElements)
            {
                var alias = aliases.NextElement ().ToString ();

                if (keystore.GetEntry (alias, protection) is KeyStore.SecretKeyEntry item)
                {
                    var bytes = item.SecretKey.GetEncoded ();

                    var serialized = System.Text.Encoding.UTF8.GetString (bytes);

                    return (alias, serialized);
                }
            }

            return (null, null);
        }
#endif


        bool saveItemToKeychain (string service, string account, string privateKey)
        {
            var context = Android.App.Application.Context;

            var password = service.ToCharArray ();

            var serviceId = $"{context.PackageName}.nomadcode.clientauth-{service}";

            var keystore = getKeystore (service);

            var item = new KeychainItem (privateKey);

            var secretEntry = new KeyStore.SecretKeyEntry (item);

            keystore.SetEntry (account, secretEntry, new KeyStore.PasswordProtection (password));

            using (var stream = context.OpenFileOutput (serviceId, FileCreationMode.Private))
            {
                keystore.Store (stream, password);
            }

            return true;
        }


        bool removeItemFromKeychain (string service)
        {
            return true;
            //throw new NotImplementedException ();
        }


        class KeychainItem : Java.Lang.Object, ISecretKey
        {
            const string raw = "RAW";

            byte [] bytes;

            public KeychainItem (string data)
            {
                if (data == null) throw new ArgumentNullException ();

                bytes = System.Text.Encoding.UTF8.GetBytes (data);
            }

            public byte [] GetEncoded () => bytes;

            public string Algorithm => raw;

            public string Format => raw;
        }
#endif
    }
}

#endif