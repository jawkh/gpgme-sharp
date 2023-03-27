using Amazon.Util.Internal.PlatformServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace PgpCombinedCrypto
{

    /// <summary>
    /// Credits: https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file 
    /// This Helper Class uses Windows Data Protection API (DPAPI) to decrypt the OpenPGP private key's Secret Passphrase. https://learn.microsoft.com/en-us/previous-versions/ms995355(v=msdn.10)?redirectedfrom=MSDN 
    /// </summary>
    internal class DecryptSecretsFromAppConfigWithWindowsDataProtectionAPI : IGetSecrets
    {
        
        static string sEntropy = Convert.ToString(ConfigurationManager.AppSettings["entropy"]);
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes(sEntropy);

        public string GetSecretString(string SecretID)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string result = appSettings[SecretID];

            if (result != null)
            {
                return ToInsecureString(DecryptString(result));
            }
            else
            {
                throw new Exception(String.Format("{0} Keys not found in AppConfig!", SecretID));
            }
        }

        private static SecureString DecryptString(string encryptedData)
        {
            try
            {
                byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(encryptedData),
                    entropy,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
                return ToSecureString(System.Text.Encoding.Unicode.GetString(decryptedData));
            }
            catch
            {
                return new SecureString();
            }
        }

        private static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }

        private static string ToInsecureString(SecureString input)
        {
            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
            return returnValue;
        }
    }

}
