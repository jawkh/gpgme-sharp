using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ProtectSecretsWithWindowsDataProtectionAPI
{

    /// <summary>
    /// Credits: https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file 
    /// This helper class uses Windows Data Protection API (DPAPI) to encrypt the Secrets. https://learn.microsoft.com/en-us/previous-versions/ms995355(v=msdn.10)?redirectedfrom=MSDN 
    /// </summary>
    public class SecretsEncryptor
    {
        /// <summary>
        /// Encrypt String using Windows Data Protection API (DPAPI)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string EncryptString(SecureString input)
        {
            string sEntropy = ConfigurationManager.AppSettings["entropy"];
            byte[] entropy = System.Text.Encoding.Unicode.GetBytes(sEntropy);

            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(Util.ToInsecureString(input)),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

    }
}
