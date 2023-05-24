using System;
using System.Text;
using System.IO;


using System.Text.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.Xml;
using System.Linq;
using OpenPgpBatchJob;
using Serilog;
//using Microsoft.Extensions.Logging;

namespace OpenPgpBatchJob
{
    class Program
    {
        static void Main(string[] args)
        {
            // Please read the README.md beforehand.
            Log.Logger = new LoggerConfiguration()
            .ReadFrom.AppSettings()
            .CreateLogger();

            Log.Information("####### WELCOME TO MOH's OpenPGP Batch Job! #######");

            OpenPgpHelper openPgpHelper = new();

            StringBuilder sb = new();
            // Loading the chosen Configurations
            if (args.Length > 0)
            {
                sb.Append("Arguments Passed by the Programmer: ");

                // To print the command line 
                // arguments using foreach loop
                foreach (Object obj in args)
                {
                    sb.AppendLine(obj.ToString());
                }

                Log.Information(sb.ToString());

                var appSettings = ConfigurationManager.AppSettings;

                try
                {
                    openPgpHelper.RuntimeAppSettings = AppConfigDictionary.ParseXmlFragmentFromFile(appSettings[args[0]]);
                    openPgpHelper.Init(); // must be called after loading the RuntimeAppSettings

                    // [Uncomment the following line of code if necessary for troubleshooting your chosen Secrets Manager]
                    // START -------------------------------------------------------------------------
                    //openPgpHelper.TestSecretsManager();
                    // END ---------------------------------------------------------------------------
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("Initialization Error! [{0}] Exiting with Error Code: -1!", ex));
                    System.Environment.Exit(-1);
                }
            }
            else
            {
                Log.Error("Error! No command line arguments found! Please supply the chosen Configuration in the argument. Eg. OpenPgpBatchJob Config_RunAsSender_for_SystemA");

                System.Environment.Exit(-1);
            }

            string sourceFolderPath = openPgpHelper.RuntimeAppSettings["SourceFolderPath"];
            string destFolderPath = openPgpHelper.RuntimeAppSettings["DestinationFolderPath"];
            string archiveFolderPath = openPgpHelper.RuntimeAppSettings["ArchiveFolderPath"];


            sb.Clear();
            sb.Append("Searching for Sender's and Recipient's PGP keys in the default keyring on this machine...");

            if (openPgpHelper.RecipientKey == null || openPgpHelper.SenderKey == null)
            {
                Log.Error("ERROR: Cannot find Sender's or Ricipient's PGP key in the your keyring! Exiting with Error Code: -1!");
                System.Environment.Exit(-1);
            }

            sb.Append("Ok!");
            Log.Information(sb.ToString());

            sb.Clear();
            sb.Append("Verifying Source and Destination Folder Paths...");

            if (!Directory.Exists(sourceFolderPath) || !Directory.Exists(destFolderPath))
            {
                Log.Error("ERROR: Cannot find Source or Destination Folder! Exiting with Error Code: -1!");
                System.Environment.Exit(-1);
            }

            sb.Append("Ok!");
            Log.Information(sb.ToString());

            if ((archiveFolderPath != null || archiveFolderPath != "") && !Directory.Exists(sourceFolderPath))
            {
                Log.Error("ERROR: Cannot find Archive Folder! Exiting with Error Code: -1!");
                System.Environment.Exit(-1);
            }

            try
            {
                int count = 0;
                string modeOfOperation = openPgpHelper.RuntimeAppSettings["ModeOfOperation"];

                if (modeOfOperation.Trim().ToUpper() == "SENDER")
                {
                    sb.Clear();
                    sb.Append(">>>>> Job is running in SENDER MODE.\n");
                    sb.Append(string.Format("Source Folder: [{0}]\nDestination Folder: [{1}]\nArchive Folder: [{2}]", sourceFolderPath, destFolderPath, archiveFolderPath ?? "Unspecified"));
                    Log.Information(sb.ToString());
                }
                else // recipient Mode
                {
                    sb.Clear();
                    sb.Append(">>>>> Job is running in RECIPIENT MODE.\n");
                    sb.Append(string.Format("Source Folder: [{0}]\nDestination Folder: [{1}]\nArchive Folder: [{2}]", sourceFolderPath, destFolderPath, archiveFolderPath ?? "Unspecified"));
                    Log.Information(sb.ToString());
                }

                ProcessFilesInFolderAndSubFolders(openPgpHelper, sourceFolderPath, destFolderPath, archiveFolderPath, ref count);

                Log.Information(string.Format("COMPLETED! [{0}] files successfully processed!", count));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("ERROR! {0}. Exiting with Error Code: -1!", ex.ToString()));
                System.Environment.Exit(-1);
            }

            //#if DEBUG
            //            Console.WriteLine("\n\nPress Enter to Exit. ");
            //            Console.ReadLine();
            //#endif

        }

        private static void ProcessFilesInFolderAndSubFolders(OpenPgpHelper openPgpHelper, string sourceFolderPath, string destFolderPath, string archiveFolderPath, ref int count)
        {
            try
            {
                string destFilePrefix = openPgpHelper.RuntimeAppSettings["DestinationFilePrefix"];
                string archiveFilePrefix = openPgpHelper.RuntimeAppSettings["ArchiveFilePrefix"];
                string modeOfOperation = openPgpHelper.RuntimeAppSettings["ModeOfOperation"];

                foreach (var (srcFilePath, destinationFilePath, archiveFilePath) in from string srcFilePath in Directory.GetFiles(sourceFolderPath)
                                                                                    let srcFileName = Path.GetFileName(srcFilePath)
                                                                                    where srcFileName != null// ie. Not a SubFolder Name, but a FileName
                                                                                    let destinationFilePath = Path.Combine(destFolderPath, string.Format("{0}{1}", destFilePrefix ?? "", srcFileName))
                                                                                    let archiveFilePath = archiveFolderPath != null ? Path.Combine(archiveFolderPath, string.Format("{0}{1}", archiveFilePrefix ?? "", srcFileName)) : null
                                                                                    select (srcFilePath, destinationFilePath, archiveFilePath))
                {
                    if (modeOfOperation.Trim().ToUpper() == "SENDER")
                    {
                        Log.Information(string.Format(">>>>> {0}. Encrypting & Signing [{1}]...", ++count, srcFilePath));
                        openPgpHelper.EncryptAndSignFile(srcFilePath, destinationFilePath, archiveFilePath);
                    }
                    else // recipient Mode
                    {
                        Log.Information(string.Format(">>>>> {0}. Decrypting & Verifying [{1}]...", ++count, srcFilePath));
                        openPgpHelper.DecryptFileAndVerifySignature(srcFilePath, destinationFilePath, archiveFilePath);
                    }
                }

                foreach (var (sourceSubFolderPath, destSubFolderPath, archiveSubFolderPath) in from string sourceSubFolderPath in Directory.GetDirectories(sourceFolderPath)
                                                                                               let subFolderName = Path.GetFileName(sourceSubFolderPath)
                                                                                               let destSubFolderPath = Path.Combine(destFolderPath, subFolderName)
                                                                                               let archiveSubFolderPath = archiveFolderPath != null ? Path.Combine(archiveFolderPath, subFolderName) : null
                                                                                               select (sourceSubFolderPath, destSubFolderPath, archiveSubFolderPath))
                {
                    if (!Directory.Exists(destSubFolderPath))
                    {
                        Directory.CreateDirectory(destSubFolderPath);
                    }

                    if (archiveSubFolderPath != null && !Directory.Exists(archiveSubFolderPath))
                    {
                        Directory.CreateDirectory(archiveSubFolderPath);
                    }

                    ProcessFilesInFolderAndSubFolders(openPgpHelper, sourceSubFolderPath, destSubFolderPath, archiveSubFolderPath, ref count);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



    }


}
