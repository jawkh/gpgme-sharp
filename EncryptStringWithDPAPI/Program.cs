
// See https://aka.ms/new-console-template for more information
using EncryptStringWithDPAPI;

Console.WriteLine("### Encrypting your Secrets using Windows Data Protection API ###\n\n");
Console.WriteLine("### Enter your Secrets and press Enter: ");

string? secretsInClear = null;

while (string.IsNullOrEmpty(secretsInClear))
{
    secretsInClear = Console.ReadLine();
}

Console.WriteLine("-----BEGIN SECRET-----\n{0}", EncryptSecretsUsingWindowsDataProtectionAPI.EncryptString(EncryptSecretsUsingWindowsDataProtectionAPI.ToSecureString(secretsInClear)));
Console.WriteLine("-----END SECRET-----");