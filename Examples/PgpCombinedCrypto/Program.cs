using System;
using System.Text;
using System.IO;

using Libgpgme;
using System.Text.Json;
using System.Collections.Generic;
using System.Configuration;

namespace PgpCombinedCrypto
{
    class Program
    {
        static void Main()
        {
            // Please read the README.md beforehand.

            // [Uncomment the following line of code if necessary for troubleshooting your chosen Secrets Manager]
            // START -------------------------------------------------------------------------
            //TestSecretsManager();
            // END ---------------------------------------------------------------------------


            Context ctx = new Context();

            if (ctx.Protocol != Protocol.OpenPGP)
                ctx.SetEngineInfo(Protocol.OpenPGP, null, null);

            Console.WriteLine("####### WELCOME TO MOH's OpenPGP Developer's Guide Demo Application! #######\n");
            Console.WriteLine("Meet Alice who is the sender and Bob who is the recipient.\n");

            // alice is the sender in this example
            // bob is the recipient in this example
            Console.WriteLine("Search Bob's and Alice's PGP keys in the default keyring on this machine...");

            var appSettings = ConfigurationManager.AppSettings;
            string aliceEmail = Convert.ToString(appSettings["AliceEmailAddress"]);
            string bobEmail = Convert.ToString(appSettings["BobEmailAddress"]);

            String[] searchpattern = new String[2]; ;
            searchpattern[0] = aliceEmail;
            searchpattern[1] = bobEmail;

            IKeyStore keyring = ctx.KeyStore;

            // We want the key signatures!
            ctx.KeylistMode = KeylistMode.Signatures;

            // retrieve all keys that have Bob's or Alice's email address
            Key[] keys = keyring.GetKeyList(searchpattern, false);

            PgpKey bob = null, alice = null;
            if (keys != null && keys.Length != 0)
            {
                foreach (Key k in keys)
                {
                    if (k.Uid != null)
                    {
                        if (bob == null && k.Uid.Email.ToLower().Equals(bobEmail))
                            bob = (PgpKey)k;
                        if (alice == null && k.Uid.Email.ToLower().Equals(aliceEmail))
                            alice = (PgpKey)k;
                    }
                    else
                        throw new InvalidKeyException();
                }
            }

            if (bob == null || alice == null)
            {
                Console.WriteLine("Cannot find Bob's or Alice's PGP key in your keyring.");
                Console.WriteLine("You may want to create the PGP keys by using the appropriate\n"
                    + "sample in the Samples/ directory.");
                return;
            }

            //// Create a sample string
            //StringBuilder randomtext = new StringBuilder();
            //for (int i = 0; i < 80 * 6; i++)
            //    randomtext.Append((char)(34 + i % 221));
            //string origintxt = new string('+', 508)
            //    + " Die Gedanken sind frei "
            //    + new string('+', 508)
            //    + randomtext;

            //// we want our string UTF8 encoded.
            //UTF8Encoding utf8 = new UTF8Encoding();

            //// Write sample string to plain.txt
            //File.WriteAllText("plain.txt", origintxt, utf8);

            string senderDataFilePath = appSettings["SenderDataFilePath"];


            Console.Write(string.Format("Alice has a Data File ({0}) that contains sensitive data that she needs to send to Bob.\n\n", senderDataFilePath));
            /////// ENCRYPT AND SIGN DATA ///////

            Console.Write(">>>>> Encrypting data for Bob and signing it with Alice's PGP key...\n");

            GpgmeData plain = new GpgmeFileData(senderDataFilePath); // There is option to create a memory based buffer for GPGME instead. That is useful if the data is loaded from object-stores or databases instead of the OS filesystem. Refer to the sample codes in the DataSampleTest project.
            GpgmeData cipher = new GpgmeFileData("cipher.asc");

            // Create ASCII armored output. The default is to create the binary OpenPGP format.
            ctx.Armor = true;

            /* Set the password callback 
             */
            ctx.SetPassphraseFunction(SenderPassphraseCallback);
            ctx.PinentryMode = PinentryMode.Loopback; // Use the Loopback option to supply the secretPassphrase programmatically
            // Set Alice's PGP key as signer
            ctx.Signers.Clear();
            ctx.Signers.Add(alice);

            EncryptionResult encrst = ctx.EncryptAndSign(
                new Key[] { bob },
                EncryptFlags.AlwaysTrust,
                plain,
                cipher);

            Console.WriteLine("Successfully Encrypted and Signed the data payload (cipher.asc)!\n\n");

            // print out invalid signature keys
            if (encrst.InvalidRecipients != null)
            {
                foreach (InvalidKey key in encrst.InvalidRecipients)
                    Console.WriteLine("Invalid key: {0} ({1})",
                        key.Fingerprint,
                        key.Reason);
            }

            plain.Close();
            cipher.Close();

            /////// DECRYPT AND VERIFY DATA ///////
            ctx.ClearPassphraseFunction();
            ctx.SetPassphraseFunction(RecipientPassphraseCallback);
            ctx.PinentryMode = PinentryMode.Loopback; // Use the Loopback option to supply the secretPassphrase programmatically

            Console.Write(">>>>> Decrypting and verifying data...\n");

            string recipientDecryptedFilePath = string.Format("{0}-DECRYPTED{1}", senderDataFilePath, Path.GetExtension(senderDataFilePath));

            cipher = new GpgmeFileData("cipher.asc"); // Filepath of the Encrypted Payload
            plain = new GpgmeFileData(recipientDecryptedFilePath); // Filepath of the decrypted payload. There is option to create a memory based buffer for GPGME instead. That is useful if the data is loaded from object-stores or databases instead of the OS filesystem. Refer to the sample codes in the DataSampleTest project.

            CombinedResult comrst = ctx.DecryptAndVerify(
                cipher, // source buffer
                plain); // destination buffer

            plain.Close();
            Console.WriteLine("Done. \n\nSource Filename: [{0}], Decrypted Filename: [{1}]\nRecipients:",
                comrst.DecryptionResult.FileName, recipientDecryptedFilePath);

            /* print out all recipients key ids (a PGP package can be 
             * encrypted to various recipients).
             */
            DecryptionResult decrst = comrst.DecryptionResult;
            if (decrst.Recipients != null)
                foreach (Recipient recp in decrst.Recipients)
                    Console.WriteLine("\tKey id {0} with {1} algorithm",
                        recp.KeyId,
                        Gpgme.GetPubkeyAlgoName(recp.KeyAlgorithm));
            else
                Console.WriteLine("\tNone");

            // print out signature information
            VerificationResult verrst = comrst.VerificationResult;
            if (verrst.Signature != null)
            {
                foreach (Signature sig in verrst.Signature)
                {
                    Console.WriteLine("Verification result (signature): "
                        + "\n\tFingerprint: {0}"
                        + "\n\tHash algorithm: {1}"
                        + "\n\tKey algorithm: {2}"
                        + "\n\tTimestamp: {3}"
                        + "\n\tSummary: {4}"
                        + "\n\tValidity: {5}",
                        sig.Fingerprint,
                        Gpgme.GetHashAlgoName(sig.HashAlgorithm),
                        Gpgme.GetPubkeyAlgoName(sig.PubkeyAlgorithm),
                        sig.Timestamp,
                        sig.Summary,
                        sig.Validity);
                }
            }
            Console.WriteLine("\n\nPress Enter to Exit. ");
            Console.ReadLine();
        }

        /// <summary>
        /// Function to test the configured Secrets Manager
        /// </summary>
        private static void TestSecretsManager()
        {
            Console.WriteLine("Testing Secrets Manager...");
            string senderSecretPassphrase = GetSenderSecretPassphrase();
            string recipientSecretPassphrase = GetRecipientSecretPassphrase();
        }

        /// <summary>
        /// Helper function to retrieve the Sender's Secret Passphrase from the configured Secrets Manager
        /// </summary>
        /// <returns>The Secret Passphrase</returns>
        private static string GetSenderSecretPassphrase()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string passphraseProtectionMode = appSettings["PassphraseProtectionMode"].Trim().ToUpper();
            string senderSecretPassphrase;

            Console.WriteLine(string.Format("Fetching Alice's Secret Passphrase programmatically [{0}]...\n\t", passphraseProtectionMode));

            switch (passphraseProtectionMode)
            {
                case "AWS_SECRETSMANAGER":
                    {
                        // Recommended for AWS serverless and containerized based solutions. Also useful for Applications hosted on AWS EC2 Instances.
                        IGetSecrets sm = new GetSecretsFromAWSSecretsManager();
                        string senderSecretPassphraseID = appSettings["AliceAWSSecretsName"]; //Set the Secret Name configured in AWS Secrets Manager
                        var retrievedSecrets = JsonSerializer.Deserialize<Dictionary<string, string>>(sm.GetSecretString(senderSecretPassphraseID));
                        senderSecretPassphrase = retrievedSecrets["SecretPassPhrase"];
                        Console.WriteLine("Fetched Secret Passphrase from AWS Secrets Manager...");
                    }
                    break;
                case "WINDOWS_DPAPI":
                    {
                        // Only works for Systems developed for Windows OS. Optimized for Windows-Based Applications! 
                        IGetSecrets sm = new DecryptSecretsFromAppConfigWithWindowsDataProtectionAPI();
                        string senderSecretPassphraseID = "AliceEncryptedSecretPassPhrase_WIND_DPAPI";
                        senderSecretPassphrase = sm.GetSecretString(senderSecretPassphraseID);
                        Console.WriteLine("Decrypted Secret Passphrase using Windows Data Protection API...");
                    }
                    break;
                case "ASPNET_DPAPI":
                default:
                    {
                        // Works for Windows, Linux and macOS based Applications. Can be used on any .NET core applications, including non-ASP.NET ones. Recommended for all other types of Applications that cannot use Solutions 1 & 2. 
                        IGetSecrets sm = new DecryptSecretsFromAppConfigWithASPNETCoreDPAPI();
                        string senderSecretPassphraseID = "AliceEncryptedSecretPassPhrase_ASP_DPAPI";
                        senderSecretPassphrase = sm.GetSecretString(senderSecretPassphraseID);
                        Console.WriteLine("Decrypted Secret Passphrase using ASP.NET Core Data Protection API...");
                    }
                    break;
            }

            return senderSecretPassphrase;
        }

        /// <summary>
        /// Helper function to retrieve the Recipient's Secret Passphrase from the configured Secrets Manager
        /// </summary>
        /// <returns></returns>
        private static string GetRecipientSecretPassphrase()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string passphraseProtectionMode = appSettings["PassphraseProtectionMode"].Trim().ToUpper();
            string recipientSecretPassphrase;
            Console.WriteLine(string.Format("Fetching Bob's Secret Passphrase programmatically [{0}]...\n\t", passphraseProtectionMode));
            switch (passphraseProtectionMode)
            {
                case "AWS_SECRETSMANAGER":
                    {
                        // Recommended for AWS serverless and containerized based solutions. Also useful for Applications hosted on AWS EC2 Instances.
                        IGetSecrets sm = new GetSecretsFromAWSSecretsManager();
                        string recipientSecretPassphraseID = appSettings["BobAWSSecretsName"]; //Set the Secret Name configured in AWS Secrets Manager
                        var retrievedSecrets = JsonSerializer.Deserialize<Dictionary<string, string>>(sm.GetSecretString(recipientSecretPassphraseID));
                        recipientSecretPassphrase = retrievedSecrets["SecretPassPhrase"];
                        Console.WriteLine("Fetched Secret Passphrase from AWS Secrets Manager...");
                    }
                    break;
                case "WINDOWS_DPAPI":
                    {
                        // Only works for Systems developed for Windows OS. Optimized for Windows-Based Applications! 
                        IGetSecrets sm = new DecryptSecretsFromAppConfigWithWindowsDataProtectionAPI();
                        string recipientSecretPassphraseID = "BobEncryptedSecretPassPhrase_WIND_DPAPI";
                        recipientSecretPassphrase = sm.GetSecretString(recipientSecretPassphraseID);
                        Console.WriteLine("Decrypted Secret Passphrase using Windows Data Protection API...");
                    }
                    break;
                case "ASPNET_DPAPI":
                default:
                    {
                        // Works for Windows, Linux and macOS based Applications. Can be used on any .NET core applications, including non-ASP.NET ones. Recommended for all other types of Applications that cannot use Solutions 1 & 2. 
                        IGetSecrets sm = new DecryptSecretsFromAppConfigWithASPNETCoreDPAPI();
                        string recipientSecretPassphraseID = "BobEncryptedSecretPassPhrase_ASP_DPAPI";
                        recipientSecretPassphrase = sm.GetSecretString(recipientSecretPassphraseID);
                        Console.WriteLine("Decrypted Secret Passphrase using ASP.NET Core Data Protection API...");
                    }
                    break;
            }

            return recipientSecretPassphrase;
        }
        /// <summary>
        /// Sender's Passphrase callback method. Invoked if a action requires the user's password.
        /// </summary>
        /// <param name="ctx">Context that has invoked the callback.</param>
        /// <param name="info">Information about the key.</param>
        /// <param name="passwd">User supplied password.</param>
        /// <returns></returns>
        public static PassphraseResult SenderPassphraseCallback(
           Context ctx,
           PassphraseInfo info,
           ref char[] passwd)
        {
            string senderSecretPassphrase = GetSenderSecretPassphrase();

            passwd = senderSecretPassphrase.ToCharArray();
            Console.WriteLine("OK!");
            return PassphraseResult.Success;
        }

        /// <summary>
        /// Recipient's Passphrase callback method. Invoked if a action requires the user's password.
        /// </summary>
        /// <param name="ctx">Context that has invoked the callback.</param>
        /// <param name="info">Information about the key.</param>
        /// <param name="passwd">User supplied password.</param>
        /// <returns></returns>
        public static PassphraseResult RecipientPassphraseCallback(
               Context ctx,
               PassphraseInfo info,
               ref char[] passwd)
        {
            string recipientSecretPassphrase = GetRecipientSecretPassphrase();
            passwd = recipientSecretPassphrase.ToCharArray();
            Console.WriteLine("OK!");
            return PassphraseResult.Success;
        }
    }
}
