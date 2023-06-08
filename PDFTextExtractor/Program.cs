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
        public static string folderPathBase = AppContext.BaseDirectory;

        public static bool recursiveFolder = false;
        public static bool overwriteTextFile = false;
        public static bool stopOnException = true;

        static void Main(string[] args)
        {
#if DEBUG
            // using a different path for debugging
            folderPathBase = @"D:\Users\MartonJr\Downloads";
#endif

            log("PDFTextExtractor - Extract texts from PDFs and write them to text file using command line");
            log("   By Niall Moran - 2020");
            log("   Edited By Marton Lyra - 2023");
            log("");

            if (args.Count() <= 1)
            {

                log("");
                log("   Parameters:");
                log("      \"/f:<PDFs folder path>\"");
                log("      /r:false or /r:true : Recursive folder - Look for PDFs recursively inside subfolders (default /r:false)");
                log("      /o:false or /o:true : Overwrite output Text File if exists (default /o:false)");
                log("      /soe:false or /soe:true : Stop on Error - If false, it will try to ignore any exception and continue looking for other PDF files");
                log("");
            }

            Arguments CommandLine = new Arguments(System.Environment.GetCommandLineArgs());

            if (CommandLine["f"] != null)
                folderPathBase = CommandLine["folderPath"];

            if (CommandLine["r"] != null)
                Boolean.TryParse(CommandLine["r"], out recursiveFolder);

            if (CommandLine["o"] != null)
                Boolean.TryParse(CommandLine["o"], out overwriteTextFile);

            if (CommandLine["soe"] != null)
                Boolean.TryParse(CommandLine["soe"], out stopOnException);

            bool folderExists = Directory.Exists(folderPathBase);
            log("PDF Text Extractor " + (folderExists ? "will" : "would") + " extract text from all PDF files in folder:");
            log("   '" + folderPathBase + "'");
            if (!folderExists)
            {
                log("But this folder does not exists.\nAborting...");
                Console.Read();
                return;
            }

            log("");
            log("The text files will be saved in the same folder as the PDF files, with the same file name but with the .txt extension.");
            log("");
            log("If the text file already exists it WILL " + (overwriteTextFile ? "" : "NOT") + "BE overwritten.");
            log("");
            log("We will " + (recursiveFolder ? "" : "NOT ") + "look for PDFs inside children folders recursively.");
            log("");
            if (!stopOnException)
                log("In case of Exceptions, I will try to continue.");
            log("");

            log("Press <enter> to continue");
            Console.Read();
            searchForPDFAndExtractText(folderPathBase);

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
                        log(string.Format("\nExtracting text from {0}", file.Name));
                        var result = extractor.Extract(file.FullName);

                        // write to a text file with the same name
                        string textFileName = string.Format("{0}{1}", file.Name.Replace(file.Extension, ""), ".txt");

                        if (!overwriteTextFile && File.Exists(textFileName))
                        {
                            log("Text file already exists. Looking for next...");
                            continue;
                        }

                        log(string.Format("Creating file '{0}'", textFileName));
                        using (StreamWriter textFile = File.CreateText(string.Format(@"{0}\{1}", folderInfo.FullName, textFileName)))
                        {
                            textFile.WriteLine(TextUtil.CleanPDFText(result.Text));
                        }
                        log("Done. " + result + "bytes written.");
                    }
                    catch (Exception ex1)
                    {
                        log("Sorry, we ran into a problem while processing file\n  '" + file.FullName + "'");
                        log(" Exception: " + ex1 + "\n\n");

                        if (stopOnException)
                        {    
                            log("Press <enter> to abort");
                            Console.Read();
                            System.Environment.Exit(255);
                        }
                            
                    }
                    
                }
            }
            catch (Exception ex2)
            {
                log("Sorry, we ran into a problem while processing folder\n  '" + currentFolder + "'");
                log(" Exception: " + ex2 + "\n\n");

                if (stopOnException)
                {
                    log("Press <enter> to abort");
                    Console.Read();
                    System.Environment.Exit(254);
                }
            }
        }

        public static void log(string text)
        {
            Console.WriteLine(text);
        }
    }
}
