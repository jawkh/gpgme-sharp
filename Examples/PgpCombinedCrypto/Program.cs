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
            // Please read the preparation section of MOH's OpenPGP Development Guide beforehand.

            // [Uncomment the following code if necessary for troubleshooting accessing your chosen Secrets Manager]
            // START -------------------------------------------------------------------------
            TestSecretsManager();
            // END ---------------------------------------------------------------------------


            Context ctx = new Context();

            if (ctx.Protocol != Protocol.OpenPGP)
                ctx.SetEngineInfo(Protocol.OpenPGP, null, null);

            Console.WriteLine("Search Bob's and Alice's PGP keys in the default keyring..");

            // Please follow the implementation guide to Create and/or Import Bob's and Alice's OpenPGP keys onto the machine beforehand
            // bob is the recipient in this example
            // alice is the sender in this example

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

            // Create a sample string
            StringBuilder randomtext = new StringBuilder();
            for (int i = 0; i < 80 * 6; i++)
                randomtext.Append((char)(34 + i % 221));
            string origintxt = new string('+', 508)
                + " Die Gedanken sind frei "
                + new string('+', 508)
                + randomtext;

            // we want our string UTF8 encoded.
            UTF8Encoding utf8 = new UTF8Encoding();

            // Write sample string to plain.txt
            File.WriteAllText("plain.txt", origintxt, utf8);

            /////// ENCRYPT AND SIGN DATA ///////

            Console.Write("Encrypt data for Bob and sign it with Alice's PGP key.. ");

            GpgmeData plain = new GpgmeFileData("plain.txt");
            GpgmeData cipher = new GpgmeFileData("cipher.asc");

            // Create ASCII armored output. The default is to create the binary OpenPGP format.
            ctx.Armor = true;
            ctx.Protocol = Protocol.OpenPGP;

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

            Console.WriteLine("done.\n\n");

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

            Console.Write("Decrypt and verify data.. ");

            cipher = new GpgmeFileData("cipher.asc"); // Filepath of the Encrypted Payload
            //plain = new GpgmeMemoryData(); // Load Decrypted Payload into Memory Stream if it is not desirable to save it to filesystem. (eg. for saving into Object Store or Database System). Refer to the sample codes in the DataSampleTest project.
            plain = new GpgmeFileData("decryptedPlain.txt"); // Filepath of the decrypted payload

            CombinedResult comrst = ctx.DecryptAndVerify(
                cipher, // source buffer
                plain); // destination buffer

            Console.WriteLine("Done. Filename: \"{0}\" Recipients:",
                comrst.DecryptionResult.FileName);

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
        }

        private static void TestSecretsManager()
        {
            string senderSecretPassphrase = GetSenderSecretPassphrase();
            string recipientSecretPassphrase = GetRecipientSecretPassphrase();
        }

        private static string GetSenderSecretPassphrase()
        {
            var appSettings = ConfigurationManager.AppSettings;
            bool IsUsingAWSSecretsManager = Convert.ToBoolean(appSettings["IsUsingAWSSecretsManager"]);
            string senderSecretPassphrase;

            if (IsUsingAWSSecretsManager)
            {
                // Systems that are hosted in AWS are strongly encouraged to use AWS Secrets Manager to secured their OpenPGP Secret Passphrase
                // Test retrieving Secrets from AWS SecretsManager
                IGetSecrets sm = new GetSecretsFromAWSSecretsManager();
                string senderSecretPassphraseID = "prod/AliceSecretPassPhrase";
                var retrievedSecrets = JsonSerializer.Deserialize<Dictionary<string, string>>(sm.GetSecretString(senderSecretPassphraseID));
                senderSecretPassphrase = retrievedSecrets["SecretPassPhrase"];

            }
            else
            {
                // Systems that are hosted outside of AWS need to manage the OpenPGP Secrets separately.
                // Systems running on Windows OS may consider using the Windows Data Protection API (DPAPI) to encrypt their OpenPGP Secret Passphrase before saving it into the AppSettings parameter
                // THIS SOLUTION IS BASED ON WINDOWS DATA PROTECTION API AND THUS ONLY WORKS FOR SYSTEMS RUNNING ON MS WINDOWS 
                IGetSecrets sm = new DecryptSecretsFromAppConfigWithWindowsDataProtectionAPI();
                string senderSecretPassphraseID = "AliceEncryptedSecretPassPhrase";
                senderSecretPassphrase = sm.GetSecretString(senderSecretPassphraseID);
            }

            return senderSecretPassphrase;
        }

        private static string GetRecipientSecretPassphrase()
        {
            var appSettings = ConfigurationManager.AppSettings;
            bool IsUsingAWSSecretsManager = Convert.ToBoolean(appSettings["IsUsingAWSSecretsManager"]);
            string recipientSecretPassphrase;

            if (IsUsingAWSSecretsManager)
            {
                // Systems that are hosted in AWS are strongly encouraged to use AWS Secrets Manager to secured their OpenPGP Secret Passphrase
                // Test retrieving Secrets from AWS SecretsManager
                IGetSecrets sm = new GetSecretsFromAWSSecretsManager();
                string recipientSecretPassphraseID = "prod/BobSecretPassPhrase";
                var retrievedSecrets = JsonSerializer.Deserialize<Dictionary<string, string>>(sm.GetSecretString(recipientSecretPassphraseID));
                recipientSecretPassphrase = retrievedSecrets["SecretPassPhrase"];

            }
            else
            {
                // Systems that are hosted outside of AWS need to manage the OpenPGP Secrets separately.
                // Systems running on Windows OS may consider using the Windows Data Protection API (DPAPI) to encrypt their OpenPGP Secret Passphrase before saving it into the AppSettings parameter
                // THIS SOLUTION IS BASED ON WINDOWS DATA PROTECTION API AND THUS ONLY WORKS FOR SYSTEMS RUNNING ON MS WINDOWS 
                IGetSecrets sm = new DecryptSecretsFromAppConfigWithWindowsDataProtectionAPI();
                string recipientSecretPassphraseID = "BobEncryptedSecretPassPhrase";
                recipientSecretPassphrase = sm.GetSecretString(recipientSecretPassphraseID);
            }
            return recipientSecretPassphrase;
        }
        /// <summary>
        /// Passphrase callback method. Invoked if a action requires the user's password.
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
            Console.Write("Supplying Sender's Secret Passphrase programmatically...");

            string senderSecretPassphrase = GetSenderSecretPassphrase();

            passwd = senderSecretPassphrase.ToCharArray();
            Console.Write("OK!");
            return PassphraseResult.Success;
        }

        /// <summary>
        /// Passphrase callback method. Invoked if a action requires the user's password.
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
            Console.Write("Supplying Recipient's Secret Passphrase programmatically...");

            string recipientSecretPassphrase = GetRecipientSecretPassphrase();
            passwd = recipientSecretPassphrase.ToCharArray();
            Console.Write("OK!");
            return PassphraseResult.Success;
        }
    }
}
