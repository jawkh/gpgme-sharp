Background
==========
This repository was forked from https://github.com/gpgme-sharp/gpgme-sharp

Adapted the original [PgpCombinedCrypto](Examples/PgpCombinedCrypto) project to provide a developer's guide for implementing OpenPGP (ie. using gpgme) for MOH and MOH's partners. Target Microsoft .NET Framework 6.0 (LTS) to support Applications that are developed for Windows, Linux and macOS.

Omited those VS.Net projects that are irrelevant in our context for brevity reasons.

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

Catalog of VS.Net Projects in this Implementation Guide Repo.
======================================================
1. [OpenPgpBatchJob](OpenPgpBatchJob) implements a fully functional Production-Ready OpenPGP Batch Job (using gnupgme-sharp). This project is written in C# and targets Microsoft .NET Framework 6.0 (LTS). 

    a. Provides a ready to use option to start implementing OpenPGP in a way that complies with the Secured Coding Rules stated in MOH's OpenPGP Implementation guide without any development efforts.

    b. Thanks to .NET 6.0, this Batch Job can be built to run on Windows, Linux and macOS Operating Systems.

    c. This Batch Job supports multiple runtime configurations (eg. Config_RunAsSender_for_SystemA, Config_RunAsRecipient_for_SystemA, etc.). This allows a single instance of the BatchJob to support multiple OpenPGP use cases - eg. as a 'SenderRole with Partner-System-A' or as a 'RecipientRole with Partner-System-A', etc. Each Execution of the Batch Job will be based on 1 chosen Runtime Configuration, specified in an input argument to run the job.

    d. This Batch Job is able to process all the files in the source folder, inclusive of the files in all the sub-folders therein. 

    e. This Batch Job is able to perform auto-archival of source files, if an archive folder path is specified in the Runtime Configuration.

    f. Logging to Console and to LogFiles.

2. [PgpCombinedCrypto](Examples/PgpCombinedCrypto) provides the sample code as part of the developer's guide for MOH and MOH's partners to implement GnuPG. 

    a. Ready-to-Use Source Code for most applications [For building OpenPGP capabilities into Web API, Serverless-based solutions, etc.]
  
    - Thanks to .NET 6.0 framework, the provided source code will be suitable to implement the recommended way to use GnuPG within an application built for Windows, Linux and macOS Operating Systems. It can also be adopted to build AWS Lambda Functions and Azure Functions.

    b. Ready-to-Use Solutions for protecting the confidentiality of OpenPGP Private Keys’ Secret Passphrases.
The provided source code provides 3 ready-to-use solutions that provides protection for the confidentiality of the secret passphrase for the OpenPGP private keys.

    - Solution 1: Uses AWS Secrets Manager [Recommended for AWS serverless and containerized based solutions. Also useful for Applications hosted on AWS EC2 Instances.]

    - Solution 2: Uses Windows Data Protection API [Only works for Systems developed for Windows OS. Optimized for Windows-Based Applications!]
    
    - Solution 3: Uses ASP.NET Core Data Protection API [Works for Windows, Linux and macOS based Applications. Can be used on any .NET core applications, including non-ASP.NET ones. Recommended for all other types of Applications that cannot use Solutions 1 & 2.

3. [ProtectSecretsWithWindowsDataProtectionAPI](ProtectSecretsWithWindowsDataProtectionAPI) provides a console program and a reusable code library for encrypting secrets by using Windows Data Protection API. This code can only be run on Windows Systems. **TIP:** This will be useful for protecting sensitive application configuration settings that are stored directly on a Windows Server when a Secrets Manager solution is not in use.

4. [ProtectSecretsWithASPNETCoreDataProtectionAPI](ProtectSecretsWithASPNETCoreDataProtectionAPI) provides a console program and a reusable code library for encrypting secrets by using ASP.NET Core Data Protection API. This code can be used on Windows, Linux and macOS Systems. **TIP:** This will be useful for protecting sensitive application configuration settings that are stored directly on a Windows/Linux/macOS Server when a Secrets Manager solution is not in use.

5. [DataBufferTest](Examples/DataBufferSamples/DataBufferTest) provides sample code on howto use gpgme-sharp's data buffers to handle the encrypted/decrypted data payloads without saving them  onto the OS filesystems. This is suitable for reading/writing the data from/to databases or object stores. This code can be used on Windows, Linux and macOS Systems.
