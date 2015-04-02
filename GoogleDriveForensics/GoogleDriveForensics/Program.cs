using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using Google.Apis.Logging;
using Google.Apis.Services;
using Google.Apis.Upload;
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
            DriveAnalyzer driveAnalyzer = await DriveAnalyzer.CreateDriveAnalyzerAysnc();
            Console.WriteLine("Authorization completed.");

            await driveAnalyzer.ListAllFilesAsync();
            Console.WriteLine();

            await driveAnalyzer.DownloadAllJsonAsync();

            Console.WriteLine("Do you want to download all files?");
            string input = Console.ReadLine();
            Console.WriteLine();
            if(input.Contains("yes"))
                await driveAnalyzer.DownloadAllFilesAsync();

            Console.WriteLine("Do you want to clear tokens?");
            input = Console.ReadLine();
            Console.WriteLine();
            if (input.Contains("yes"))
                await driveAnalyzer.Authorizer.ClearCredential();
        }
    }
}
