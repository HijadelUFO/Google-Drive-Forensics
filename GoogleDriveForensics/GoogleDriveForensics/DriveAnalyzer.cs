using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveForensics
{
    public class DriveAnalyzer
    {
        private DriveService driveService;

        //GoogleAuthorizer is used to complete authentication and authorization
        private GoogleAuthorizer authorizer;
        public GoogleAuthorizer Authorizer { get { return authorizer; } }

        //Google RESTful API base uri
        private const string BASE_URI = "https://www.googleapis.com/drive/v2/files/";
        //Folder to store downloaded files
        private const string folderPath = @"F:\Digital Forensics\Thesis\Result\Record";

        
        //Use factory pattern to build and initialize object.
        //Because async method can not be used in constructor.
        private DriveAnalyzer() { }

        private async Task<DriveAnalyzer> InitializeAsync()
        {
            authorizer = new GoogleAuthorizer();
            driveService = await authorizer.AuthorizeAndGetDriveService();
            return this;
        }

        public static async Task<DriveAnalyzer>  CreateDriveAnalyzerAysnc()
        {
            DriveAnalyzer analyzer = new DriveAnalyzer();
            return await analyzer.InitializeAsync();
        }


        //Write important metadata of all files to a txt file.
        public async Task ListAllFilesAsync()
        {
            Console.WriteLine("Listing Files...");

            using (var stream = System.IO.File.Create(Path.Combine(folderPath, "result.txt"))) { }

            BatchProcessor batch = new BatchProcessor(driveService);
            await batch.BatchListOnlyAsync(file => ListFileMetadata(file));
        }

        //Download all JSON files as metadata record
        public async Task DownloadAllJsonAsync()
        {
            BatchProcessor batch = new BatchProcessor(driveService);

            //Use Lambda expression
            await batch.BatchProcessAsync(file => DownloadJsonAsync(file));
        }

        //Download all files with content
        public async Task DownloadAllFilesAsync()
        {
            BatchProcessor batch = new BatchProcessor(driveService);

            Console.WriteLine("Start downloading...");
            Console.WriteLine();
            await batch.BatchProcessAsync(file => DownloadFileAsync(file));
        }

        private void ListFileMetadata(Google.Apis.Drive.v2.Data.File returnedFile)
        {
            using (StreamWriter writer = System.IO.File.AppendText(Path.Combine(folderPath, "result.txt")))
            {
                writer.WriteLine("File ID: " + returnedFile.Id);
                writer.WriteLine("Title: " + returnedFile.Title);
                writer.WriteLine("Original Filename: " + returnedFile.OriginalFilename);
                writer.WriteLine("Md5Checksum: " + returnedFile.Md5Checksum);
                writer.WriteLine("File Size: " + returnedFile.FileSize);
                writer.WriteLine("MIME type: " + returnedFile.MimeType);
                writer.WriteLine("Created Date: " + returnedFile.CreatedDate);
                writer.WriteLine("Modified Date: " + returnedFile.ModifiedDate);
                writer.WriteLine("Last Modifying User: " + returnedFile.LastModifyingUser.DisplayName);
                writer.WriteLine("Last Viewed By Me Date: " + returnedFile.LastViewedByMeDate);

                writer.Write(writer.NewLine);
                writer.WriteLine("Shared: " + returnedFile.Shared);
                if (returnedFile.SharingUser != null)
                    writer.WriteLine("Sharing User: " + returnedFile.SharingUser.DisplayName);
                writer.WriteLine("Last modified by me: " + returnedFile.ModifiedByMeDate);
                writer.WriteLine("Download URL: " + returnedFile.DownloadUrl);

                writer.WriteLine("Explicitly Trashed: " + returnedFile.ExplicitlyTrashed);

                writer.Write(writer.NewLine);
                writer.WriteLine("======================================");
                writer.Write(writer.NewLine);

                Console.WriteLine(returnedFile.Title + " recorded.");
            }
        }

        private async Task DownloadJsonAsync(Google.Apis.Drive.v2.Data.File returnedFile)
        {
            //Generate URI to file resource
            Uri fileUri = new Uri(BASE_URI + returnedFile.Id);

            try
            {
                //Use HTTP client in DriveService to obtain response
                Task<Stream> fileJsonStream = driveService.HttpClient.GetStreamAsync(fileUri);
                Console.WriteLine("Downloading JSON file of {0}...", returnedFile.Title);

                using (Stream jsonStream = await fileJsonStream)
                {
                    //Folder path to download JSON files
                    string jsonFolderPath = Path.Combine(folderPath, "JSON");
                    if (!Directory.Exists(jsonFolderPath))
                        Directory.CreateDirectory(jsonFolderPath);

                    //Write response stream to file
                    using (FileStream output = System.IO.File.Open(
                        Path.Combine(jsonFolderPath, returnedFile.Title + ".json"), FileMode.Create))
                    {
                        Task writeFile = jsonStream.CopyToAsync(output);
                        Console.WriteLine("Writing JSON file of {0}...", returnedFile.Title);
                        await writeFile;
                        Console.WriteLine(returnedFile.Title + ".json has been written to disk.");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while downloading JSON file: " + e.Message);
            }
        }

        private async Task DownloadFileAsync(Google.Apis.Drive.v2.Data.File returnedFile)
        {
            //Not every file resources in Google Drive has file content.
            //For example, a folder
            if (returnedFile.FileSize == null)
            {
                Console.WriteLine(returnedFile.Title + " is not a file. Skipped.");
                Console.WriteLine();
                return;
            }

            //Generate URI to file resource
            //"alt=media" indicates downloading file content instead of JSON metadata
            Uri fileUri = new Uri(BASE_URI + returnedFile.Id + "?alt=media");

            try
            {
                //Use HTTP client in DriveService to obtain response
                Task<Stream> fileJsonStream = driveService.HttpClient.GetStreamAsync(fileUri);
                Console.WriteLine("Downloading file {0}...", returnedFile.Title);

                using (Stream fileStream = await fileJsonStream)
                {
                    string fileName = Path.Combine(folderPath, returnedFile.Title);

                    //Write response stream to file
                    using (FileStream output = System.IO.File.Open(fileName, FileMode.Create))
                    {
                        Task writeFile = fileStream.CopyToAsync(output);
                        Console.WriteLine("-----Writing file {0}...", returnedFile.Title);
                        await writeFile;
                        Console.WriteLine("-----" + returnedFile.Title + " has been written to disk.");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while downloading JSON file: " + e.Message);
            }
        }
    }
}
