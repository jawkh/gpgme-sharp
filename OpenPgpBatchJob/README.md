Intro
=====
This project implements a fully functional Production-Ready OpenPGP Batch Job (using gnupgme-sharp). 
Provides a ready to use option to start implementing OpenPGP in a way that complies with the Secured Coding Rules stated in MOH's OpenPGP Implementation guide without any development efforts .
This project is written in C# and targets Microsoft .NET Framework 6.0 (LTS). Thanks to .NET 6.0, this Batch Job can be built to run on Windows, Linux and macOS Operating Systems. 

Key Features
1. Ready-to-Use Production-Ready BatchJob that implements OpenPGP in accordance to MOH's OpenPGP Specifications. 

2. Thanks to .NET 6.0, this Batch Job can be built to run on Windows, Linux and macOS Operating Systems.

3. This Batch Job supports multiple runtime configurations (eg. Config_RunAsSender_for_SystemA, Config_RunAsRecipient_for_SystemA, etc.). This allows a single instance of this BatchJob to support multiple OpenPGP use cases - eg. as a 'SenderRole with Partner-System-A' or as a 'RecipientRole with Partner-System-A', etc. Each Execution of the Batch Job will be based on 1 chosen Runtime Configuration, specified in an input argument to run the job.

4. This Batch Job is able to process all the files in the source folder, inclusive of the files in all the sub-folders therein. 

5. This Batch Job is able to perform auto-archival of source files, if an archive folder path is specified in the Runtime Configuration.

6. Logging to Console and to LogFiles.

Preparations (Follow the steps in Sequence to setup your development environment to run this code.)
===================================================================================================
Refer to [SETUP.docx](SETUP.docx) for instructions on the following steps.

1. Beforehand, please install the GnuPG requirements for the gpgme-sharp NuGet package on your development machine. 

2. Begin by Building the Solution (F6). You should be able to successfully build the solution without any errors.

3. Generate the OpenPGP Keypairs. Enable secret passphrase for each of them. 

4. Decide on the method for protecting the confidentiality of the secret passphrase. 
   This repository provides source code for 3 ready-to-use solutions that protects the confidentiality of the secret passphrases of the OpenPGP private keys.

     a.	Uses AWS Secrets Manager [Recommended for AWS serverless and containerized based solutions. Also useful for Applications hosted on AWS EC2 Instances.]

     b.	Uses Windows Data Protection API [Only works for Systems developed for Windows OS. Optimized for Windows-Based Applications!]

     c.	Uses ASP.NET Core Data Protection API [Works for Windows, Linux and macOS based Applications. Can be used on any .NET core applications, including non-ASP.NET ones. Recommended for all other types of Applications that cannot use Solutions 1 & 2.]
   
   Refer to the respective steps in SETUP.docx to enable each of the option above. 

5. Execution of the BatchJob. 

    a. Option 1: Testing. You may run the sample code in Debug Mode / Without Debug Mode (F5 / Ctrl + F5). 
    b. Option 2: Production. Execute the console program with an input argument specifying the configuration to use. Eg. OpenPgpBatchJob Config_RunAsRecipient_for_SystemA


© 2023 jonathan_aw@moh.gov.sg