using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using sun.util.resources.cldr.bn;
using org.omg.CORBA;
using TikaOnDotNet.TextExtraction;
using PDFTextExtractor.Util;
using System.Diagnostics;

namespace PDFTextExtractor
{
    /// <summary>
    /// 
    /// PDFTextExtractor is a straightforward DotNet console application designed to extract text from multiple PDF files.
    /// 
    /// It was initially developed by Niall Moran in 2020 and has been edited by Marton Lyra in 2023.
    /// 
    /// </summary>
    class Program
    {
        // the folder to look for PDF files
        public static string inputPDFFolder = AppContext.BaseDirectory;
        public static string outputTXTFolder = "";

        public static bool recursiveFolder = false;
        public static bool overwriteTextFile = false;
        public static bool stopOnException = true;
        public static bool pauseOnException = true;
        
        public static bool logToFile = true;
        public static string logFileName = "";
        public static StringBuilder logTemp = new StringBuilder();

        public static long totalFilesRead = 0;
        public static long totalPDFExtractedWithSuccess = 0;
        public static long totalPDFExtractedWithError = 0;
        public static long totalPDFIgnored = 0;
        public static long totalTXTOverriten = 0;
        public static long totalExceptions = 0;
        public static Stopwatch stopwatch = new Stopwatch();

        public static List<string> ignoredDirs = new List<string>();

        public static TextExtractor textExtractor;

        static void Main(string[] args)
        {
            log("\nPDFTextExtractor - Extract texts from PDFs and write them to text file using command line");
            log("   By Niall Moran - 2020              - https://github.com/niallermoran/PDFTextExtractor");
            log("   New features By Marton Lyra - 2023 - https://github.com/MartonLyra/PDFTextExtractor");
            log("");

            if (args.Count() <= 1)
            {

                log("");
                log("   Parameters:");
                log("      \"/i:<input PDFs folder path>\" - Specifies the folder path where the PDF files are stored");
                log("      \"/o:<output TXTs folder path>\" - (default same as input folder)");
                log("");
                log("      /r:false or /r:true : Recursive folder - Look for PDFs recursively inside subfolders (default /r:false)");
                log("      /w:false or /w:true : Overwrite output Text File if exists (default /w:false)");
                log("      /l:false or /l:true : Log console to file in output folder (default /l:true)");
                log("");
                log("      /ignoreDir:<folders ignored> : folders to ignore in source dir - semicolon separated");
                log("");
                log("      /soe:false or /soe:true : Stop on Error - If false, it will try to ignore any exception and continue looking for other PDF files (default /soe:true)");
                log("      /poe:false or /poe:true : Pause on Error - If true, it will ask for <enter> key to continue in case of Exceptions (default /poe:true)");
                log("");
            }

            Arguments CommandLine = new Arguments(System.Environment.GetCommandLineArgs());

            // Input PDF folder:
            if (CommandLine["i"] != null)
                inputPDFFolder = CommandLine["i"];

            // Output TXT folder:
            if (CommandLine["o"] != null)
                outputTXTFolder = CommandLine["o"];
            else
                outputTXTFolder = inputPDFFolder;

            // Recursive search:
            if (CommandLine["r"] != null)
                Boolean.TryParse(CommandLine["r"], out recursiveFolder);

            // Overwrite output Text File
            if (CommandLine["w"] != null)
                Boolean.TryParse(CommandLine["w"], out overwriteTextFile);

            // Folders to ignore - semicolon separated
            if (CommandLine["ignoreDir"] != null)
            {
                string ignoreDirCommand = CommandLine["ignoreDir"];
                List<string> tempIgnoredDirs = ignoreDirCommand.Split(';').ToList<string>();
                log("Ignoring folders containing any of the following text:");
                foreach (var item in tempIgnoredDirs)
                {
                    string tempItem = item.Trim();
                    log($"  '{tempItem}'");
                    ignoredDirs.Add(tempItem);
                }
                log("");
            }


            // Stop on Exception
            if (CommandLine["soe"] != null)
                Boolean.TryParse(CommandLine["soe"], out stopOnException);

            // Pause on Exception
            if (CommandLine["poe"] != null)
                Boolean.TryParse(CommandLine["poe"], out pauseOnException);

#if DEBUG
            // using a different path for debugging
            // inputPDFFolder = @"D:\PDFInput-Delme";
            // outputTXTFolder = @"D:\PDFOutput-Delme";
#endif


            // Log to file in output folder (default /l:true)
            if (CommandLine["l"] != null)
                Boolean.TryParse(CommandLine["l"], out logToFile);
            if (!logToFile)
                logTemp.Clear();
            else
            {
                // Calculating LOG fileName:
                logFileName = Path.Combine(
                    outputTXTFolder,
                    String.Format("{0:yyyy-MM-dd HH-mm}", DateTime.Now) + " - PDF Text Extractor.log"
                );
            }






            bool inputPDFFolderExists = Directory.Exists(inputPDFFolder);
            bool outputTXTFolderExists = Directory.Exists(outputTXTFolder);
            log("PDF Text Extractor " + ((inputPDFFolderExists && outputTXTFolderExists) ? "will" : "would") + " extract text from all PDF files in folder:");
            log("   '" + inputPDFFolder + "'");
            if (!inputPDFFolderExists)
            {
                log("But the input PDF folder does not exists:\n" + inputPDFFolder + "\nAborting...");
                Console.Read();
                return;
            }
            if (!outputTXTFolderExists)
            {
                log("But the output TXT folder does not exists:\n" + outputTXTFolder + "\nAborting...");
                Console.Read();
                return;
            }

            log("");
            if (inputPDFFolder.Equals(outputTXTFolderExists))
                log("The text files will be saved in the same folder as the PDF files, with the same file name but with the .txt extension.");
            else
            {
                log("The text files will be saved with the same tree folder, with the same file name but with the .txt extension.");
            }
            log("");
            log("If the text file already exists it WILL " + (overwriteTextFile ? "" : "NOT ") + "BE overwritten.");
            log("");
            log("We will " + (recursiveFolder ? "" : "NOT ") + "look for PDFs inside children folders recursively.");
            log("");
            if (!stopOnException)
                log("In case of Exceptions, I will try to continue.");

            log("");
            log(" Input PDF Folder: " + inputPDFFolder);
            log("Output TXT Folder: " + outputTXTFolder);
            log("");
            log("Press <enter> to continue");
            Console.Read();

            textExtractor = new TextExtractor();
            stopwatch.Start();
            searchForPDFAndExtractText(inputPDFFolder);

            Console.Title = "All Done!";

            showStatistics();

            log("Press <enter> to finish");
            Console.Read();
        }


        /// <summary>
        /// Search for PDF and extract text;
        /// </summary>
        /// <param name="currentFolder">Folder to look for</param>
        private static void searchForPDFAndExtractText(string currentFolder)
        {
            // Check if this is a user ignored folder:
            if (shouldIgnoreDir(currentFolder))
            {
                log("Ignoring folder: " + currentFolder);
                return;
            }

            try
            {
                // get folder info
                DirectoryInfo folderInfo = new DirectoryInfo(currentFolder);

                // Let´s first get inside sub-folders:
                if (recursiveFolder)
                {
                    log("Sub-Folders of: " + currentFolder);
                    foreach (DirectoryInfo subFolders in folderInfo.EnumerateDirectories())
                        searchForPDFAndExtractText(subFolders.FullName);
                }
                    

                log("Folder: " + currentFolder);

                // Read every pdf in currentFolder
                foreach (FileInfo file in folderInfo.EnumerateFiles("*.pdf"))
                {
                    bool retry = false;
                    
                    do
                    {
                        try
                        {
                            Console.Title = $"[{String.Format("{0:#,##0}", totalFilesRead)}] - {file.FullName}";

                            // Log DateTime + Total files read + PDF Full path name
                            log($"[{String.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now)} - {String.Format("{0:#,##0}", totalFilesRead)}] Extracting text from '{file.Name}'");
                            var result = textExtractor.Extract(file.FullName);

                            // write to a text file with the same name but different extension:
                            string textFileName = string.Format("{0}{1}", file.Name.Replace(file.Extension, ""), ".txt");

                            // Replacing input folder with output folder:
                            string folderDestination = folderInfo.FullName.ReplaceInsensitive(inputPDFFolder, outputTXTFolder);
                            if (!Directory.Exists(folderDestination))
                                Directory.CreateDirectory(folderDestination);

                            // Destiny text file already exists?
                            if (File.Exists(Path.Combine(folderDestination, textFileName)))
                            {
                                string msg = "Text file already exists. ";
                                if (overwriteTextFile)
                                {
                                    totalTXTOverriten++;
                                    log(msg + "Overwriting...");
                                }
                                else
                                {
                                    totalPDFIgnored++;
                                    log(msg + "Ignoring.\n");
                                    continue;
                                }
                            }



                            log(string.Format("Creating file '{0}'", textFileName));
                            using (StreamWriter textFile = File.CreateText(string.Format(@"{0}\{1}", folderDestination, textFileName)))
                            {
                                textFile.WriteLine(TextUtil.CleanPDFText(result.Text));
                            }
                            log("");

                            totalPDFExtractedWithSuccess++;
                        }
                        catch (Exception ex1)
                        {
                            totalExceptions++;
                            totalPDFExtractedWithError++;
                            showStatistics();
                            log("Sorry, we ran into a problem while processing file\n  '" + file.FullName + "'");
                            log(" Exception: " + ex1 + "\n\n");

                            retry = RetryStopOrPauseExecution(true);

                            if (retry)
                                log("Let´s try it again...\n");
                        }

                    } while (retry);

                    totalFilesRead++;

                    if (totalFilesRead % 250 == 0)
                        showStatistics();
                }
            }
            catch (Exception ex2)
            {
                totalExceptions++;
                log("Sorry, we ran into a problem while processing folder\n  '" + currentFolder + "'");
                log(" Exception: " + ex2 + "\n\n");

                RetryStopOrPauseExecution();
            }
        }

        /// <summary>
        /// Check if current dir should be ignored comparing with user command parameter
        /// </summary>
        /// <param name="currentDir">current dir 2 check</param>
        /// <returns>true if should ignore or false if should not ignore this folder</returns>
        public static bool shouldIgnoreDir(string currentDir)
        {
            if (ignoredDirs.Count <= 0)
                return false;

            // Allows filter by folders "ending with" instead of only "contains"
            if (!currentDir.EndsWith(@"\"))
                currentDir = currentDir + @"\";

            foreach (string dir2ignore in ignoredDirs)
            {
                if (currentDir.ContainsIgnoreCase(dir2ignore))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Write log text to console and save to file if configured by user
        /// </summary>
        /// <param name="text"></param>
        public static void log(string text)
        {
            Console.WriteLine(text);
            
            if (logToFile)
            {
                logTemp.AppendLine(text);
                 
                if (!"".Equals(logFileName))
                try
                {
                    File.AppendAllText(logFileName, logTemp.ToString(), Encoding.Unicode);
                    logTemp.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception on save log to file:");
                    Console.WriteLine(ex.ToString());


                    // Do not call StopOrPauseExecution() - no log() use:
                    if (stopOnException)
                    {
                        Console.WriteLine("Press <enter> to abort");
                        Console.Read();
                        System.Environment.Exit(254);
                    }

                    if (pauseOnException)
                    {
                        Console.WriteLine("Press <enter> to continue...");
                        Console.Read();
                    }
                }
            }
        }


        /// <summary>
        /// Depending on user configuration we will stop or pause execution;
        /// </summary>
        /// <param name="askForRetry">Should we ask for user to retry if functionality is avalable</param>
        /// <returns>Returns true means 'retry'</returns>
        private static bool RetryStopOrPauseExecution(bool askForRetry = false)
        {
            if (stopOnException)
            {
                log("Press <enter> to abort");
                Console.Read();
                System.Environment.Exit(254);
            }

            if (pauseOnException)
            {
                log($"Press {(askForRetry? "<r> to Retry or any other key" : "<enter>")} to to continue...");
                if (askForRetry)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.KeyChar.ToString().EqualsIgnoreCase("R"))
                        return true;
                }
                else
                {
                    Console.ReadLine();
                }
                
            }
            return false;
        }

        /// <summary>
        /// Log process statistics
        /// </summary>
        private static void showStatistics()
        {
            log("\n" + String.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now));
            log($"Total Files Read: {totalFilesRead}");
            log($"Total PDF Extracted With Success: {totalPDFExtractedWithSuccess}");
            log($"Total PDF Extracted With Error: {totalPDFExtractedWithError}");
            log($"Total PDF Ignored: {totalPDFIgnored}");
            log($"Total TXT Overriten: {totalTXTOverriten}");
            log($"Total Exceptions: {totalExceptions}");
            log("");
        }
    }
}
