Intro
=====
This project implements a fully functional Production-Ready OpenPGP Batch Job (using gnupgme-sharp). 
Provides a ready to use option to start implementing OpenPGP in a way that complies with the Secured Coding Rules stated in MOH's OpenPGP Implementation guide without any development efforts.
This project is written in C# and targets Microsoft .NET Framework 6.0 (LTS). Thanks to .NET 6.0, this Batch Job can be built to run on Windows, Linux and macOS Operating Systems. 

Key Features
1. Ready-to-Use Production-Ready BatchJob that implements OpenPGP in accordance to MOH's OpenPGP Specifications. 

2. Thanks to .NET 6.0, this Batch Job can be built to run on Windows, Linux and macOS Operating Systems.

3. This Batch Job supports multiple Scenario configurations (eg. Config_RunAsSender_for_SystemA, Config_RunAsRecipient_for_SystemA, etc.). This allows a single instance of this BatchJob to support multiple OpenPGP use cases - eg. as a 'SenderRole with Partner-System-A' or as a 'RecipientRole with Partner-System-A', etc. Each Execution of the Batch Job will be based on 1 chosen Scenario Configuration, specified in an input argument to run the job.

4. This Batch Job can process all the files in the source folder, inclusive of the files in all the sub-folders therein. 

5. This Batch Job can perform auto-archival of source files, if an archive folder path is specified in the Runtime Configuration.

6. Logging to Console and to Log Files.


Options to obtain the Batch job:
================================
1.	Download the Latest Binary Release for Windows: Download the latest release-built exe of MOH OpenPGP Batch job Program for Windows OS. Choose this if you do not intend to customize the Batch Job.

2.	Build the Batch Job from Source Code: Choose this if you either intend to customize the Batch Job or need to build the Batch Job for Non-Windows OS.

Refer to [SETUP.docx](SETUP.docx) for further instructions.



© 2023 jonathan_aw@moh.gov.sg