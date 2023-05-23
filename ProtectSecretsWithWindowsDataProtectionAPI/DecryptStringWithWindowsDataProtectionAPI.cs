using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Security;

namespace ProtectSecretsWithWindowsDataProtectionAPI
{
    

    /// <summary>
    /// This helper class uses Windows Data Protection API (DPAPI) to Decrypt the Secrets 
    /// Credits: https://simplecodesoftware.com/articles/how-to-encrypt-data-on-macos-without-dpapi 
    /// </summary>
    public class SecretsDecryptor
    {
        /// <summary>
        /// Decrypt String using Windows Data Protection API (DPAPI)
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static SecureString DecryptString(string secret)
        {
            string sEntropy = ConfigurationManager.AppSettings["entropy"];
            return DecryptString(secret, sEntropy);
        }

        /// <summary>
        /// Decrypt String using Windows Data Protection API (DPAPI)
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static SecureString DecryptString(string secret, string sEntropy)
        {
            
            byte[] entropy = System.Text.Encoding.Unicode.GetBytes(sEntropy);

            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(secret),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return Util.ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }
    }
}
