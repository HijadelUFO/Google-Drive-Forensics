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

namespace GoogleDriveForensics
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("New Drive API Sample:");
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
                    new[] { DriveService.Scope.Drive },
                    "user", CancellationToken.None, new FileDataStore("DriveDocuments"));
            }

            // Create the service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Drive API Sample",
            });

            await ListFiles(service);
        }

        private async Task ListFiles(DriveService service)
        {
            Console.WriteLine("Listing Files... (Execute ASYNC)");
            Console.WriteLine("======================================\n");

            var fileLst = await service.Files.List().ExecuteAsync();

            if (fileLst.Items == null)
            {
                Console.WriteLine("No file found.");
            }
            else
            {
                foreach (Google.Apis.Drive.v2.Data.File returnedFile in fileLst.Items)
                {
                    Console.WriteLine("File ID: " + returnedFile.Id);
                    Console.WriteLine("Title: " + returnedFile.Title);
                    Console.WriteLine("Md5Checksum: " + returnedFile.Md5Checksum);
                    Console.WriteLine("File Size: " + returnedFile.FileSize);
                    Console.WriteLine("Original Filename: " + returnedFile.OriginalFilename);
                    Console.WriteLine("Created Date: " + returnedFile.CreatedDate);
                    Console.WriteLine("Modified Date: " + returnedFile.ModifiedDate);
                    Console.WriteLine("Last Modifying User: " + returnedFile.LastModifyingUser.DisplayName);
                    Console.WriteLine("Last Modifying User Name: " + returnedFile.LastModifyingUserName);
                    Console.WriteLine("Last Viewed By Me Date: " + returnedFile.LastViewedByMeDate);
                    Console.WriteLine("Editable: " + returnedFile.Editable);
                    if (returnedFile.ExplicitlyTrashed != null)
                        Console.WriteLine("Explicitly Trashed: " + returnedFile.ExplicitlyTrashed);
                    else
                        Console.WriteLine("Explicitly Trashed: Not trashed");
                    Console.WriteLine("\n======================================\n");
                }
            }
        }
    }
}
