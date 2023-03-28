
// See https://aka.ms/new-console-template for more information
using ProtectSecretsWithWindowsDataProtectionAPI;

Console.WriteLine("### Encrypting your Secrets using Windows Data Protection API ###\n\n");
Console.WriteLine("### Enter your Secrets and press Enter: ");

string? secretsInClear = null;

while (string.IsNullOrEmpty(secretsInClear))
{
    secretsInClear = Console.ReadLine();
}

string encryptedString = SecretsEncryptor.EncryptString(Util.ToSecureString(secretsInClear));

Console.WriteLine("\n-----BEGIN SECRET-----\n{0}", encryptedString); 
Console.WriteLine("-----END SECRET-----");

Console.WriteLine(string.Format("\n\nDecrypted Payload [For Verifications]: \n{0}", Util.ToInsecureString(SecretsDecryptor.DecryptString(encryptedString))));

Console.WriteLine("\n\nPress Enter to Exit. ");
Console.ReadLine();


