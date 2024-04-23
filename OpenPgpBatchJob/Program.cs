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
using System.Reflection;
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
                Log.Error($"ERROR: Cannot find Sender's PGP Key ({Convert.ToString(openPgpHelper.RuntimeAppSettings["SenderEmailAddress"]).Trim()}) or Recipient's PGP key ({Convert.ToString(openPgpHelper.RuntimeAppSettings["RecipientEmailAddress"]).Trim()}) in the keyring! Exiting with Error Code: -1!");
                System.Environment.Exit(-1);
            }

            sb.Append("Ok!");
            Log.Information(sb.ToString());

            Log.Information(string.Format("Sender's {0}", PrintPropertiesOfOpenPGPKey(openPgpHelper.SenderKey)));
            Log.Information(string.Format("Recipient's {0}", PrintPropertiesOfOpenPGPKey(openPgpHelper.RecipientKey)));

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
                int success = 0;
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

                ProcessFilesInFolderAndSubFolders(openPgpHelper, sourceFolderPath, destFolderPath, archiveFolderPath, ref count, ref success);

                Log.Information(string.Format("COMPLETED! [{0}] out of [{1}] files successfully processed!", success, count));
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

        private static void ProcessFilesInFolderAndSubFolders(OpenPgpHelper openPgpHelper, string sourceFolderPath, string destFolderPath, string archiveFolderPath, ref int count, ref int success)
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
                    try
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
                        ++success; // increment success counter
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message); // Do Not Abort Batch Job on failure to process a single file. 
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

                    ProcessFilesInFolderAndSubFolders(openPgpHelper, sourceSubFolderPath, destSubFolderPath, archiveSubFolderPath, ref count, ref success);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// Print the Properties of an PgpKey object and the properties of all its subkeys
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string PrintPropertiesOfOpenPGPKey(Libgpgme.PgpKey key)
        {
            // Printing the Key Properties
            var keyPropertiesToPrint = new List<string> { "Uid", "KeyId", "Fingerprint", "Protocol", "Revoked", "Expired", "Disabled", "Invalid", "IsQualified", "OwnerTrust", "KeylistMode", "CanAuthenticate", "CanCertify", "CanEncrypt", "CanSign", "Secret" };
            StringBuilder sb = new StringBuilder();
            sb.Append("Key Properties: ");
            sb.AppendLine(PrintProperties(key, keyPropertiesToPrint).ToString());

            // Printing the SubKeys properties
            var subKeyPropertiesToPrint = new List<string> { "KeyId", "Fingerprint", "Timestamp", "TimestampUTC", "Revoked", "Expired", "Expires", "ExpiresUTC", "Disabled", "Invalid", "IsQualified", "IsInfinitely", "CanAuthenticate", "CanCertify", "CanEncrypt", "CanSign", "Curve", "Secret", "Length", "PubkeyAlgorithm" };
            sb.AppendLine("Subkey 1 Properties: ");
            sb.AppendLine(PrintProperties(key.Subkeys, subKeyPropertiesToPrint).ToString());

            if (key.Subkeys.Next != null)
            {
                sb.AppendLine("Subkey 2 Properties: ");
                sb.AppendLine(PrintProperties(key.Subkeys, subKeyPropertiesToPrint).ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// This function print out the list of specified properties of an Object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyNames"></param>
        private static StringBuilder PrintProperties(object obj, List<string> propertyNames)
        {
            StringBuilder sb = new StringBuilder();

            if (obj == null)
            {
                Console.WriteLine("Object is null.");
                return null;
            }

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (string propName in propertyNames)
            {
                PropertyInfo property = type.GetProperty(propName);

                if (property != null)
                {
                    try
                    {
                        object value = property.GetValue(obj, null);
                        sb.AppendLine($"{property.Name} = {value}");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"{property.Name} could not be read: {ex.Message}");
                    }
                }
                else
                {
                    sb.AppendLine($"Property '{propName}' does not exist on {type.Name}.");
                }
            }

            return sb;
        }
    }


}



