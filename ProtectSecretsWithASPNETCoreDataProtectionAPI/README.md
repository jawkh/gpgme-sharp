Intro
=====
This project provides a console program and a reusable code library for encrypting and decrypting secrets by using ASP.NET Core Data Protection API, which can be used on Windows, Linux and macOS based Systems.
This project is written in C# and targets Microsoft .NET Framework 6.0 (LTS). 

**TIP:** This will be useful for protecting sensitive application configuration settings that are stored directly on a Windows/Linux/macOS Server when a Secrets Manager solution is not in use.

IMPORTANT – SETUP FOR HOSTING ENVIRONMENT:
==========================================
The ASP.NET Core Data Protection API provides data protection for apps running on a single machine. ASP.NET Core Data Protection uses its data protection Master key to encrypt and decrypt protected data, but it doesn’t protect the key itself. On Windows, ASP.NET Core Data Protection encrypts the key using DPAPI. Since DPAPI isn’t available on Linux and macOS, the key is unencrypted and stored as plaintext. If a hacker or another user steals the key, they would be able to decrypt the application data.

Fortunately, ASP.NET Core Data Protection provides developers with multiple ways to encrypt the keys at rest. 

The ProtectSecretsWithASPNETCoreDataProtectionAPI console program and code library use X.509 certificate to encrypt the ASP.NET Core Data Protection Master key at rest. This program will auto-generate a Self-Signed SSL Cert and import it into the Current User’s Personal Cert Store for this purpose. 

Hence, it is imperative to run the ProtectSecretsWithASPNETCoreDataProtectionAPI Console Program (to encrypt your secret) on each server, using the same service-account that will be used to run your own Application (ie. which performs the decryptions). Also ensure that you set the same value for **entropy** and **SSLCertDistinguishedSubjectName** AppSettings variables for the ProtectSecretsWithASPNETCoreDataProtectionAPI console program and for your own Application.

The encrypted passphrase can only be decrypted on the same server where it was originally encrypted. 

If there is a need to rotate the SSL Cert that was used to protect the Master key of ASP.NET Core Data Protection API on your Server, simply delete away the existing one from the Current User’s Personal Cert Store and rerun the ProtectSecretsWithASPNETCoreDataProtectionAPI Console Program to encrypt your secrets again. The console program will auto-regenerate and re-import a replacement SSL Cert as it performs your encryption operation. 

Once you delete away the old SSL Cert, you will not be able to decrypt the previously encrypted Secret PassPhrases anymore. Thereafter, please configure the newly encrypted secrets for your own Application that is running on that server. 
