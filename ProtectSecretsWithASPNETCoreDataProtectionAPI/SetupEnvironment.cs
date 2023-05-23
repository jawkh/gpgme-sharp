using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using static System.Net.Mime.MediaTypeNames;
using System.Configuration;


namespace ProtectSecretsWithASPNETCoreDataProtectionAPI
{
    internal class SetupEnvironment
    {

        /// <summary>
        /// Generate Self-Signed Cert. 
        /// This can be used to encrypt the ASP.NET Core Data Protection Key on your Machine.
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns></returns>
        private static X509Certificate2 CreateSelfSignedDataProtectionCertificate(string subjectName)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                CertificateRequest request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);
                X509Certificate2 certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddYears(2));
                return certificate;
            }
        }

        /// <summary>
        /// Install the self-signed certificate as non-exportable to prevent stealing.
        /// </summary>
        /// <param name="certificate"></param>
        private static void InstallCertificateAsNonExportable(X509Certificate2 certificate)
        {
            byte[] rawData = certificate.Export(X509ContentType.Pkcs12, password: "");

            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.MaxAllowed))
            {
                //store.Certificates.Import(rawData, password: "password123", keyStorageFlags: X509KeyStorageFlags.PersistKeySet);
                store.Add(new X509Certificate2(rawData));
            }
        }

        

        /// <summary>
        /// The X509KeyStorageFlags.PersistKeySet flag saves the certificate into the login KeyChain on the OS. This way, the application can access the certificate and decrypt the data on the next run.
        /// Make sure that the application uses the same certificate between runs:
        /// </summary>
        /// <returns></returns>
        private static X509Certificate2 SetupDataProtectionCertificate(string sslCertDistinguishedSubjectName)
        {
            //string subjectName = "CN=ASP.NET Core Data Protection Certificate";

            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadOnly))
            {
                X509Certificate2Collection certificateCollection = store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName,
                    sslCertDistinguishedSubjectName,
                    // self-signed certificate won't pass X509 chain validation
                    validOnly: false);
                if (certificateCollection.Count > 0)
                {
                    return certificateCollection[0];
                }

                X509Certificate2 certificate = CreateSelfSignedDataProtectionCertificate(sslCertDistinguishedSubjectName);
                InstallCertificateAsNonExportable(certificate);
                return certificate;
            }
        }

        ///// <summary>
        ///// Configure ASP.NET Core Data Protection on your Machine
        ///// </summary>
        ///// <param name="serviceCollection"></param>
        //internal static void ConfigureServices(IServiceCollection serviceCollection)
        //{
        //    string dataProtectionKeysDirectory = Path.Combine(
        //                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        //                        "OsEncryption-Keys");

        //    X509Certificate2 dataProtectionCertificate = SetupDataProtectionCertificate();


        //    serviceCollection.AddDataProtection()
        //        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysDirectory))
        //        .ProtectKeysWithCertificate(dataProtectionCertificate);
        //    //Console.Write(String.Format("ASP.NET Data Protection Key Encrypted and saved to [{0}]...", dataProtectionKeysDirectory));

        //}

        /// <summary>
        /// Configure ASP.NET Core Data Protection on your Machine
        /// </summary>
        /// <param name="serviceCollection"></param>
        internal static void ConfigureServices(IServiceCollection serviceCollection, string sslCertDistinguishedSubjectName)
        {
            string dataProtectionKeysDirectory = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "OsEncryption-Keys");

            X509Certificate2 dataProtectionCertificate = SetupDataProtectionCertificate(sslCertDistinguishedSubjectName);


            serviceCollection.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysDirectory))
                .ProtectKeysWithCertificate(dataProtectionCertificate);
            //Console.Write(String.Format("ASP.NET Data Protection Key Encrypted and saved to [{0}]...", dataProtectionKeysDirectory));

        }
    }

}

