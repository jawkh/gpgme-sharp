Release Notes for MOH OpenPGP Batch Job [for Windows]
=====================================================

Version: 1.0.0
==============
First stable release of this batchjob. 

**Key Features**

1. Ready-to-Use Production-Ready BatchJob that implements OpenPGP in accordance to MOH's OpenPGP Specifications. 

2. Thanks to .NET 6.0, this Batch Job can be built to run on Windows, Linux and macOS Operating Systems.

3. This Batch Job supports multiple Scenario configurations (eg. Config_RunAsSender_for_SystemA, Config_RunAsRecipient_for_SystemA, etc.). This allows a single instance of this BatchJob to support multiple OpenPGP use cases - eg. as a 'SenderRole with Partner-System-A' or as a 'RecipientRole with Partner-System-A', etc. Each Execution of the Batch Job will be based on 1 chosen Scenario Configuration, specified in an input argument to run the job.

4. This Batch Job can process all the files in the source folder, inclusive of the files in all the sub-folders therein. 

5. This Batch Job can perform auto-archival of source files, if an archive folder path is specified in the Runtime Configuration.

6. Logging to Console and to LogFiles.


© 2023 jonathan_aw@moh.gov.sg