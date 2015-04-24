using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Collections.Generic;

namespace GoogleDriveForensics
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Google Drive Forensics:");
            Console.WriteLine("================================");
            try
            {
                new Program().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }
        }

        private async Task Run()
        {
            string FOLDER_PATH = @"F:\Digital Forensics\Thesis\Result\Record";

            DriveScanner scanner = await DriveScanner.CreateDriveScannerAysnc(FOLDER_PATH);

            bool quit = false;
            while(!quit)
            {
                Console.WriteLine("=======================");
                Console.WriteLine("Available actions:");
                Console.WriteLine("1. List all files.");
                Console.WriteLine("2. Download summary.");
                Console.WriteLine("3. Download all metadata.");
                Console.WriteLine("4. Download all file contents.");
                Console.WriteLine("5. Check file revisions.");
                Console.WriteLine("q. Quit program.");
                Console.WriteLine("=======================");

                Console.Write("Enter your action: ");
                string answer = Console.ReadLine();

                switch(answer)
                {
                    case "1":
                        Console.WriteLine("Listing all files...");
                        await scanner.printFileListAsync();
                        break;
                    case "2":
                        Console.WriteLine("Downloading summary...");
                        await scanner.Donwloader.DownloadSummaryAsync();
                        Console.WriteLine("Summary downloaded.");
                        break;
                    case "3":
                        Console.WriteLine("Downloading all metadata records...");
                        await scanner.Donwloader.DownloadAllMetadataAsync();
                        Console.WriteLine("All metadata downloaded.");
                        break;
                    case "4":
                        Console.WriteLine("Downloading all file contents...");
                        await scanner.Donwloader.DownloadAllContentsAsync();
                        Console.WriteLine("All file contents downloaded.");
                        break;
                    case "5":
                        Console.WriteLine("Listing numbers of each file's revisions...");
                        var entryRevisionList = await scanner.listRevisionsAsync();
                        int index = 1;
                        foreach(var entry in entryRevisionList)
                        {
                            Console.WriteLine("{0}. {1} => {2}", 
                                index++, entry.EntryTitle, entry.EntryRevisionCount);
                        }
                        Console.WriteLine();

                        while (true)
                        {
                            int num;
                            while (true)
                            {
                                Console.WriteLine("Please enter the index of a file to downlod revisions.");
                                Console.WriteLine("--or--");
                                Console.WriteLine("Enter \'q\' to return to upper level menu.");
                                string input = Console.ReadLine();

                                num = -1;
                                if (Char.IsLetter(input[0])) break;
                                num = Int32.Parse(input);

                                if ((num > 0 && num <= entryRevisionList.Count))
                                    break;
                                else
                                    Console.WriteLine("Please enter a correct number.");
                            }
                            if (num == -1) break;
                            await scanner.Donwloader.downloadAllRevisions(entryRevisionList[num - 1].EntryID);
                        }
                        break;
                    case "q":
                        quit = true;
                        break;
                    default:
                        Console.WriteLine("Please enter a valid option.");
                        break;
                }

                Console.WriteLine();
            }

            Console.WriteLine("Do you want to clear tokens before quiting?");
            string clear = Console.ReadLine();
            Console.WriteLine();
            if (clear.StartsWith("y", true, null))
                await scanner.ClearTokens();
        }
    }
}
