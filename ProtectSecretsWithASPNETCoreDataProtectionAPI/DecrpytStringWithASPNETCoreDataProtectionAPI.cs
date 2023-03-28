using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace ProtectSecretsWithASPNETCoreDataProtectionAPI
{
    /// <summary>
    /// This helper class uses ASP.NET Core Data Protection API to Decrypt Secrets. 
    /// This helper class supports Windows, Linus and macOS Systems! 
    /// Credits: https://simplecodesoftware.com/articles/how-to-encrypt-data-on-macos-without-dpapi 
    /// </summary>
    public class SecretsDecryptor
    {
        /// <summary>
        /// Decrypt String using ASP.NET Core Data Protection API
        /// </summary>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static SecureString DecryptString(string secret)
        {
            string sEntropy = ConfigurationManager.AppSettings["entropy"];
            var serviceCollection2 = new ServiceCollection();
            SetupEnvironment.ConfigureServices(serviceCollection2);
            IDataProtector dataProtector2 = serviceCollection2.BuildServiceProvider().GetDataProtector(purpose: sEntropy);

            byte[] unprotectedSecretBytes = dataProtector2.Unprotect(Convert.FromBase64String(secret));
            return Util.ToSecureString(System.Text.Encoding.Unicode.GetString(unprotectedSecretBytes, 0, unprotectedSecretBytes.Length));
        }
    }
}
