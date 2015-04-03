using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveForensics
{
    class DriveAnalyzer
    {
        //Folder to store downloaded files
        private string FOLDER_PATH = @"F:\Digital Forensics\Thesis\Result\Record";

        private DriveScanner driveScanner;

        //Empty default constructor
        private DriveAnalyzer() { }

        //Use factory pattern to build and initialize object.
        //Because async method can not be used in constructor.
        private async Task<DriveAnalyzer> InitializeAsync()
        {
            driveScanner = await DriveScanner.CreateDriveScannerAysnc();
            return this;
        }

        //Static method to create and return DriveAnalyzer object
        public static async Task<DriveAnalyzer> CreateDriveAnalyzer()
        {
            DriveAnalyzer analyzer = new DriveAnalyzer();
            Console.WriteLine("Authorization completed.");
            Console.WriteLine();

            return await analyzer.InitializeAsync();
        }


        public async Task ClearTokens()
        {
            await driveScanner.Authorizer.ClearCredential();
        }

        //Write important metadata of all files to a txt file.
        public async Task ListAllFilesAsync()
        {
            Console.WriteLine("Listing Files...");

            using (var stream = System.IO.File.Create(Path.Combine(FOLDER_PATH, "result.txt"))) { }

            await driveScanner.BatchListOnlyAsync(file => ListFileMetadata(file));
        }

        private void ListFileMetadata(Google.Apis.Drive.v2.Data.File fileEntry)
        {
            using (StreamWriter writer = System.IO.File.AppendText(Path.Combine(FOLDER_PATH, "result.txt")))
            {
                writer.WriteLine("File ID: " + fileEntry.Id);
                writer.WriteLine("Title: " + fileEntry.Title);
                writer.WriteLine("Original Filename: " + fileEntry.OriginalFilename);
                writer.WriteLine("Md5Checksum: " + fileEntry.Md5Checksum);
                writer.WriteLine("File Size: " + fileEntry.FileSize);
                writer.WriteLine("MIME type: " + fileEntry.MimeType);
                writer.WriteLine("Created Date: " + fileEntry.CreatedDate);
                writer.WriteLine("Modified Date: " + fileEntry.ModifiedDate);
                writer.WriteLine("Last Modifying User: " + fileEntry.LastModifyingUser.DisplayName);
                writer.WriteLine("Last Viewed By Me Date: " + fileEntry.LastViewedByMeDate);

                writer.Write(writer.NewLine);
                writer.WriteLine("Shared: " + fileEntry.Shared);
                if (fileEntry.SharingUser != null)
                    writer.WriteLine("Sharing User: " + fileEntry.SharingUser.DisplayName);
                writer.WriteLine("Last modified by me: " + fileEntry.ModifiedByMeDate);
                writer.WriteLine("Download URL: " + fileEntry.DownloadUrl);

                writer.WriteLine("Explicitly Trashed: " + fileEntry.ExplicitlyTrashed);

                writer.Write(writer.NewLine);
                writer.WriteLine("======================================");
                writer.Write(writer.NewLine);

                Console.WriteLine(fileEntry.Title + " recorded.");
            }
        }


        //Download metadata of a file via file entry ID
        public async Task DownloadMetadataAsync(string fileId)
        {
            Google.Apis.Drive.v2.Data.File file = await driveScanner.getFileEntryAsync(fileId);

            await DownloadMetadataAsync(file);
        }

        //Download content of a file via file entry ID
        public async Task DownloadContentAsync(string fileId)
        {
            Google.Apis.Drive.v2.Data.File file = await driveScanner.getFileEntryAsync(fileId);

            await DownloadContentAsync(file);
        }

        //Download metadata of a file via file entry
        public async Task DownloadMetadataAsync(Google.Apis.Drive.v2.Data.File fileEntry)
        {
            try
            {
                //Use HTTP client in DriveService to obtain response
                Task<Stream> fileJsonStreamTask = driveScanner.GetMetaDataStreamAsync(fileEntry);

                using(Stream jsonStream = await fileJsonStreamTask)
                {
                    if (jsonStream != null)
                        await WriteStreamToFile(jsonStream,
                            Path.Combine(FOLDER_PATH, "Metadata"), fileEntry.Title + ".json");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while writing JSON file: " + e.Message);
            }
        }

        //Download content of a file via file entry
        public async Task DownloadContentAsync(Google.Apis.Drive.v2.Data.File fileEntry)
        {
            try
            {
                //Use HTTP client in DriveService to obtain response
                Task<Stream> fileContentStreamTask = driveScanner.GetContentStreamAsync(fileEntry);

                using (Stream contentStream = await fileContentStreamTask)
                {
                    if (contentStream != null)
                        await WriteStreamToFile(contentStream,
                            Path.Combine(FOLDER_PATH, "Content"), fileEntry.Title);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while writing file: " + e.Message);
            }
        }

        //Download all JSON files as metadata record
        public async Task DownloadAllMetadataAsync()
        {
            await driveScanner.BatchProcessAsync(file => DownloadMetadataAsync(file));
        }

        //Download all file contents
        public async Task DownloadAllContentsAsync()
        {
            await driveScanner.BatchProcessAsync(file => DownloadContentAsync(file));
        }

        private async Task WriteStreamToFile(Stream stream, string folderPath, string filename)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            //Write response stream to file
            using (FileStream output = System.IO.File.Open(Path.Combine(folderPath, filename), FileMode.Create))
            {
                Task writeFile = stream.CopyToAsync(output);
                Console.WriteLine("-----Writing {0}...", filename);
                await writeFile;
                Console.WriteLine("-----" + filename + " has been written to disk.");
            }
            Console.WriteLine();
        }
    }
}
