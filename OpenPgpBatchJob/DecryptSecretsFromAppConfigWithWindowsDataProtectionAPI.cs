using Amazon.Util.Internal.PlatformServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using ProtectSecretsWithWindowsDataProtectionAPI;

namespace PgpCombinedCrypto
{

    /// <summary>
    /// This Helper Class uses Windows Data Protection API (DPAPI) to decrypt the OpenPGP private key's Secret Passphrase. https://learn.microsoft.com/en-us/previous-versions/ms995355(v=msdn.10)?redirectedfrom=MSDN 
    /// LIMITATION! This helper class only supports Windows Systems!
    /// Credits: https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file 
    /// </summary>
    internal class DecryptSecretsFromAppConfigWithWindowsDataProtectionAPI 
    {
        /// <summary>
        /// Fetch and Decrypt OpenPGP Private Key's Secret Passphrase
        /// </summary>
        /// <param name="SecretValue"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetSecretString(string SecretValue, string Entropy)
        {
            return Util.ToInsecureString(SecretsDecryptor.DecryptString(SecretValue, Entropy));
        }

    }

}
