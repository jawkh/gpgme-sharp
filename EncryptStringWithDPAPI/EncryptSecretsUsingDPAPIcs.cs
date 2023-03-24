using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace EncryptStringWithDPAPI
{

    // Credits: https://weblogs.asp.net/jongalloway/encrypting-passwords-in-a-net-app-config-file 
    internal class EncryptSecretsUsingWindowsDataProtectionAPI
    {
        static byte[] entropy = System.Text.Encoding.Unicode.GetBytes("Salt Is Not A Password");

        internal static string EncryptString(System.Security.SecureString input)
        {
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                System.Text.Encoding.Unicode.GetBytes(ToInsecureString(input)),
                entropy,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }

        internal static string ToInsecureString(SecureString input)
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

        internal static SecureString ToSecureString(string input)
        {
            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }
            secure.MakeReadOnly();
            return secure;
        }
    }
}
