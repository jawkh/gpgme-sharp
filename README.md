Background
==========
This repository was forked from https://github.com/gpgme-sharp/gpgme-sharp

Adapted the original [PgpCombinedCrypto](Examples/PgpCombinedCrypto) project to provide a developer's guide for implementing gpg for MOH and MOH's partners. 

Omited unnecessary VS.Net projects for brevity reasons.

gpgme-sharp
===========

gpgme-sharp is a C# wrapper around [GPGME](https://wiki.gnupg.org/APIs), the recommended way to use GnuPG within an application. It supports .NET Framework 4.0 and higher, and [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) 2.0 (including .NET Core 2.0).

[![NuGet version](http://img.shields.io/nuget/v/gpgme-sharp.svg)](https://www.nuget.org/packages/gpgme-sharp/)&nbsp;

Requirements
============

- On Windows, you will need to install [Gpg4Win](https://www.gpg4win.org). 
- On Debian and Ubuntu, install the [libgpgme11 package](https://packages.debian.org/stretch/libgpgme11).
- On other Linux distros or other operating systems, install libgpgme using your favourite package manager, or compile it from source. 

Note that Gpg4Win currently only distributes a 32-bit build, so on Windows you **must** set your C# app to run in 32-bit mode.

Usage
=====

The library can be installed using NuGet:
```
dotnet add package gpgme-sharp
```

See the [Examples](Examples/) directory in this repo for usage examples.

Catalog of VS.Net Projects in this Repo.
======================================================
1. [PgpCombinedCrypto](Examples/PgpCombinedCrypto) provides the sample code as part of the developer's guide for MOH and MOH's partners to implement GnuPG. 

    a. Ready-to-Use Source Code for most applications
  
    - Thanks to .NET 6.0 framework, the provided source code will be suitable to be adopted by any applications built for Windows, Linux and macOS Operating Systems. It can also be adopted to build AWS Lambda Functions and Azure Functions.

    b. Ready-to-Use Solutions for protecting the confidentiality of OpenPGP Private Keys’ Secret Passphrases
The provided source code provides 2 ready-to-use solutions that provides protection for the confidentiality of the secret passphrase for the OpenPGP private keys.

    - Solution 1: Uses AWS Secrets Manager to store the OpenPGP SecretPassphrases. (Highly recommended for systems deployed in AWS)

    - Solution 2: Uses Windows Data Protection API (DPAPI) to encrypt/decrypt the OpenPGP Secet Passphrases stored on the Application Server itself. But this only supports Systems implemented on Windows OS. However, you may refer to https://simplecodesoftware.com/articles/how-to-encrypt-data-on-macos-without-dpapi for a workaround that provides a similar solution for Systems on Linux and macOS.


2. [EncryptStringWithWindowsDataProtectionAPI](EncryptStringWithDPAPI) provides a console program for encrypting the secret passphrases of OpenPGP private keys by using Windows Data Protection API. This is for one of the solution to provide confidentiality to the secret passphrases.

3. [DataBufferTest](\Examples/DataBufferSamples) provides sample code on howto use data buffers to handle the encrypted/decrypted data payload without saving them  onto the OS filesystems. This is suitable for reading/writing the data from/to databases or object stores.  