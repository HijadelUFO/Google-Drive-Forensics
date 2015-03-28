using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveForensics
{
    class DriveAnalysis
    {
        public DriveAnalysis(UserCredential cred, DriveService serv)
        {
            credential = cred;
            service = serv;
        }

        public async Task ListAllFilesAsync()
        {
            Console.WriteLine("Listing Files...");
            Console.WriteLine("======================================\n");

            var fileLst = await service.Files.List().ExecuteAsync();

            if (fileLst.Items == null)
            {
                Console.WriteLine("No file found.");
            }
            else
            {
                using (StreamWriter writer = System.IO.File.CreateText(@"F:\Digital Forensics\Thesis\Result\Record\result.txt"))
                {
                    foreach (Google.Apis.Drive.v2.Data.File returnedFile in fileLst.Items)
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
                        if(returnedFile.SharingUser != null)
                            writer.WriteLine("Sharing User: " + returnedFile.SharingUser.DisplayName);
                        writer.WriteLine("Last modified by me: " + returnedFile.ModifiedByMeDate);
                        writer.WriteLine("Download URL: " + returnedFile.DownloadUrl);

                        if (returnedFile.ExplicitlyTrashed != null)
                            writer.WriteLine("Explicitly Trashed: " + returnedFile.ExplicitlyTrashed);
                        else
                            writer.WriteLine("Explicitly Trashed: Not trashed");

                        writer.Write(writer.NewLine);
                        writer.WriteLine("======================================");
                        writer.Write(writer.NewLine);

                        Console.WriteLine("One file recorded.");
                    }
                    writer.Write(writer.NewLine);
                }

                var allFiles = fileLst.Items;
                for (int i = 0; i < allFiles.Count; i++)
                {
                    Console.WriteLine("{0} {1}", i, allFiles[i].Title);
                }
                Console.WriteLine();

            }
        }

        public void DownloadJson()
        {
            var fileList = service.Files.List().Execute();

            if (fileList.Items == null)
            {
                Console.WriteLine("No file found.");
                return;
            }

            var allFiles = fileList.Items;

            foreach (Google.Apis.Drive.v2.Data.File returnedFile in allFiles)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                    new Uri("https://www.googleapis.com/drive/v2/files/" + returnedFile.Id));
                request.Headers.Add("Authorization", credential.Token.TokenType + " " + credential.Token.AccessToken);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Downloading JSON file of {0}...", returnedFile.Title);
                    Stream resStream = response.GetResponseStream();

                    using (FileStream output = System.IO.File.Open(@"F:\Digital Forensics\Thesis\Result\Record\JSON\"
                        + returnedFile.Title + ".json", FileMode.Create))
                    {
                        resStream.CopyTo(output);
                    }
                    Console.WriteLine("Done!");
                }
                else
                    Console.WriteLine("Not OK!");
                Console.WriteLine();
            }
        }

        public void DownloadAllFiles()
        {
            var fileList = service.Files.List().Execute();

            if (fileList.Items == null)
            {
                Console.WriteLine("No file found.");
                return;
            }

            var allFiles = fileList.Items;

            foreach (Google.Apis.Drive.v2.Data.File returnedFile in allFiles)
            {
                if (returnedFile.FileSize == null)
                {
                    Console.WriteLine(returnedFile.Title + " is not a file. Skipped.");
                    continue;
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                    new Uri("https://www.googleapis.com/drive/v2/files/" + returnedFile.Id + "?alt=media"));
                request.Headers.Add("Authorization", credential.Token.TokenType + " " + credential.Token.AccessToken);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Downloading {0}...", returnedFile.Title);
                    Stream resStream = response.GetResponseStream();

                    string fileName = @"F:\Digital Forensics\Thesis\Result\Record\"
                        + returnedFile.Title;

                    using(FileStream output = System.IO.File.Open(fileName, FileMode.Create))
                    {
                        resStream.CopyTo(output);
                    }
                    Console.WriteLine("Done!");
                }
                else
                    Console.WriteLine("Not OK!");
                Console.WriteLine();
            }
        }

        private UserCredential credential;
        private DriveService service;
    }
}
