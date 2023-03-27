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
1. [PgpCombinedCrypto](Examples/PgpCombinedCrypto) provides the sample code as part of the developer's guide for MOH and MOH's partners to implement GnuPG. Provides 2 ready-to-use solutions to provide confidentiality to the secret passphrases of the OpenPGP private keys. [Sol 1: Uses AWS Secrets manager. Sol 2: Uses Windows Data Protection API.]

2. [EncryptStringWithWindowsDataProtectionAPI](EncryptStringWithDPAPI) provides a console program for encrypting the secret passphrases of OpenPGP private keys by using Windows Data Protection API. This is for one of the solution to provide confidentiality to the secret passphrases.

3. [DataBufferTest](\Examples/DataBufferSamples) provides sample code on howto use data buffers to handle the encrypted/decrypted data payload without saving them  onto the OS filesystems. This is suitable for reading/writing the data from/to databases or object stores.  