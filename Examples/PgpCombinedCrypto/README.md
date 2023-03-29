Intro
=====
This is the main project that provides the Developer's Guide for implementing OpenPGP (using gnupgme-sharp). 
This project is written in C# and targets Microsoft .NET Framework 6.0 (LTS). Thanks to .NET 6.0, the provided source code can be adopted by any applications built for Windows, Linux and macOS Operating Systems. It can also be used to build AWS Lambda Functions and Azure Functions.

Preparations (Follow the steps in Sequence to setup your development environment to run this code.)
===================================================================================================

1. Beforehand, please install the GnuPG requirements for the gpgme-sharp NuGet package on your development machine. 

2. Begin by Building the Solution (F6). You should be able to successfully build the solution without any errors.

3. Generate and import 2 sets of OpenPGP Keypairs with same cipher specifications - for the sender (alice) and the recipient (bob) in this sample code. Enable secret passphrase for each of them. 

4. Decide on the method for protecting the confidentiality of the secret passphrase. 
   This repository provides source code for 3 ready-to-use solutions that protects the confidentiality of the secret passphrases of the OpenPGP private keys.

     a.	Uses AWS Secrets Manager [Recommended for AWS serverless and containerized based solutions. Also useful for Applications hosted on AWS EC2 Instances.]

     b.	Uses Windows Data Protection API [Only works for Systems developed for Windows OS. Optimized for Windows-Based Applications!]

     c.	Uses ASP.NET Core Data Protection API [Works for Windows, Linux and macOS based Applications. Can be used on any .NET core applications, including non-ASP.NET ones. Recommended for all other types of Applications that cannot use Solutions 1 & 2.]
   
   Refer to the respective steps in SETUP.docx to enable each of the option above. 

5. You may run the sample code in Debug Mode / Without Debug Mode (F5 / Ctrl + F5) after you completed the required steps in [SETUP.docx](SETUP.docx). 
