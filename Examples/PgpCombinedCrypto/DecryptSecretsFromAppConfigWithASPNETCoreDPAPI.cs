using PgpCombinedCrypto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtectSecretsWithASPNETCoreDataProtectionAPI;

namespace PgpCombinedCrypto
{
    /// <summary>
    /// This helper class uses ASP.NET Core Data Protection API to decrypt the OpenPGP private key's Secret Passphrase. 
    /// This helper class supports Windows, Linus and macOS!
    /// Credits: https://simplecodesoftware.com/articles/how-to-encrypt-data-on-macos-without-dpapi 
    /// </summary>
    internal class DecryptSecretsFromAppConfigWithASPNETCoreDPAPI : IGetSecrets
    {
        /// <summary>
        /// Fetch and Decrypt OpenPGP Private Key's Secret Passphrase
        /// </summary>
        /// <param name="SecretID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetSecretString(string SecretID)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string result = appSettings[SecretID];

            if (result != null)
            {
                return Util.ToInsecureString(SecretsDecryptor.DecryptString(result));
            }
            else
            {
                throw new Exception(String.Format("{0} Keys not found in AppConfig!", SecretID));
            }
        }
    }
}
