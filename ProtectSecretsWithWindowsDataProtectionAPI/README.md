Intro
=====
This project provides a console program and a reusable code library for encrypting and decrypting secrets by using Windows Data Protection API, which can only be used on Windows Systems.
This project is written in C# and targets Microsoft .NET Framework 6.0 (LTS). 

**TIP:** This will be useful for protecting sensitive application configuration settings that are stored directly on a Windows Server when a Secrets Manager solution is not in use.

IMPORTANT – SETUP FOR HOSTING ENVIRONMENT:
==========================================
The Windows Data Protection API (DPAPI) is focused on providing data protection for each windows user accounts. This means that the encryption and decryption operations must be done using the same windows account. 
https://learn.microsoft.com/en-us/previous-versions/ms995355(v=msdn.10)?redirectedfrom=MSDN 

Hence, it is imperative to run the ProtectSecretsWithWindowsDataProtectionAPI Console Program (to encrypt your secret) individually on each Windows server, using the same service-account that will be used to run your own Application (ie. which performs the decryptions). Also ensure that you set the same value for **entropy** AppSettings variable for the ProtectSecretsWithWindowsDataProtectionAPI console program and for your own Application.

The encrypted passphrase can only be decrypted on the same Windows server where it was originally encrypted.
