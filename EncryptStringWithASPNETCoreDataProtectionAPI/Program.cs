// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;
using System.Text;
using System;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using ProtectSecretsWithASPNETCoreDataProtectionAPI;

Console.WriteLine("### Encrypting your Secrets using ASP.NET Core Data Protection API ###\n\n");
Console.WriteLine("### Enter your Secrets and press Enter: ");

string? secretsInClear = null;

while (string.IsNullOrEmpty(secretsInClear))
{
    secretsInClear = Console.ReadLine();
}

string encryptedString = SecretsEncryptor.EncryptString(Util.ToSecureString(secretsInClear));
Console.WriteLine("\n-----BEGIN SECRET-----\n{0}", encryptedString); ;
Console.WriteLine("-----END SECRET-----");


Console.WriteLine(string.Format("\n\nDecrypted Payload [For Verifications]: \n{0}", Util.ToInsecureString(SecretsDecryptor.DecryptString(encryptedString))));

Console.WriteLine("\n\nPress Enter to Exit. ");
Console.ReadLine();
