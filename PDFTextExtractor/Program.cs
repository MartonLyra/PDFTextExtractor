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

namespace PDFTextExtractor
{
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


        static void Main(string[] args)
        {
            log("PDFTextExtractor - Extract texts from PDFs and write them to text file using command line");
            log("   By Niall Moran - 2020");
            log("   Edited By Marton Lyra - 2023");
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
                log("      /l:false or /l:true : Log to file in output folder (default /l:true)");
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

            // Log to file in output folder (default /l:true)
            if (CommandLine["l"] != null)
                Boolean.TryParse(CommandLine["l"], out logToFile);


            // Stop on Exception
            if (CommandLine["soe"] != null)
                Boolean.TryParse(CommandLine["soe"], out stopOnException);

            // Pause on Exception
            if (CommandLine["poe"] != null)
                Boolean.TryParse(CommandLine["poe"], out pauseOnException);

#if DEBUG
            // using a different path for debugging
            inputPDFFolder = @"D:\PDFInput-Delme";
            outputTXTFolder = @"D:\PDFOutput-Delme";
#endif

            // Calculating LOG fileName:
            if (logToFile)
            {
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
            searchForPDFAndExtractText(inputPDFFolder);

            Console.Title = "Done!";

            log("Press <enter> to finish");
            Console.Read();
        }

        private static void searchForPDFAndExtractText(string currentFolder)
        {
            Console.Title = currentFolder;

            TextExtractor extractor = new TextExtractor();

            // get a list of PDF files in this directory
            try
            {
                // get the folder this executable is running in
                DirectoryInfo folderInfo = new DirectoryInfo(currentFolder);

                // Let´s first get inside sub-folders:
                if (recursiveFolder)
                {
                    log("Sub-Folders of: " + currentFolder);
                    foreach (DirectoryInfo subFolders in folderInfo.EnumerateDirectories())
                        searchForPDFAndExtractText(subFolders.FullName);
                }
                    

                log("Folder: " + currentFolder);

                foreach (FileInfo file in folderInfo.EnumerateFiles("*.pdf"))
                {
                    try
                    {
                        log(string.Format("Extracting text from {0}", file.Name));
                        var result = extractor.Extract(file.FullName);

                        // write to a text file with the same name but different extension:
                        string textFileName = string.Format("{0}{1}", file.Name.Replace(file.Extension, ""), ".txt");

                        // Replacing input folder with output folder:
                        string folderDestination = folderInfo.FullName.ReplaceInsensitive(inputPDFFolder, outputTXTFolder);
                        if (!Directory.Exists(folderDestination))
                            Directory.CreateDirectory(folderDestination);

                        if (!overwriteTextFile && File.Exists(Path.Combine(folderDestination, textFileName)))
                        {
                            log("Text file already exists. Ignoring...\n");
                            continue;
                        }

                        log(string.Format("Creating file '{0}'", textFileName));
                        using (StreamWriter textFile = File.CreateText(string.Format(@"{0}\{1}", folderDestination, textFileName)))
                        {
                            textFile.WriteLine(TextUtil.CleanPDFText(result.Text));
                        }
                        log("Done: " + textFileName);
                    }
                    catch (Exception ex1)
                    {
                        log("Sorry, we ran into a problem while processing file\n  '" + file.FullName + "'");
                        log(" Exception: " + ex1 + "\n\n");

                        StopOrPauseExecution();
                    }
                    
                }
            }
            catch (Exception ex2)
            {
                log("Sorry, we ran into a problem while processing folder\n  '" + currentFolder + "'");
                log(" Exception: " + ex2 + "\n\n");

                StopOrPauseExecution();
            }
        }

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

        private static void StopOrPauseExecution()
        {
            if (stopOnException)
            {
                log("Press <enter> to abort");
                Console.Read();
                System.Environment.Exit(254);
            }

            if (pauseOnException)
            {
                log("Press <enter> to continue...");
                Console.Read();
            }
        }
    }
}
