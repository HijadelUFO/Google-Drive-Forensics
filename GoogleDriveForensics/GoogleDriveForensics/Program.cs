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
            Console.WriteLine("Google Drive API Sample:");
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
            UserCredential credential;

            using (var stream = new System.IO.FileStream("client.json",
                System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                Console.WriteLine("JSON file open.");
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { DriveService.Scope.DriveReadonly },
                    "user", CancellationToken.None, new FileDataStore("DriveDocuments"));
            }

            // Create the service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Drive API Sample",
            });

            DriveAnalyzer driveAnalyzer = new DriveAnalyzer(service);

            await driveAnalyzer.ListAllFilesAsync();
            Console.WriteLine();

            await driveAnalyzer.DownloadAllJsonAsync();

            Console.WriteLine("Do you want to download all files?");
            string input = Console.ReadLine();
            Console.WriteLine();
            if(input.Contains("yes"))
                await driveAnalyzer.DownloadAllFilesAsync();
        }
    }
}
