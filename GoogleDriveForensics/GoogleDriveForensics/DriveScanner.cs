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
    public class DriveScanner
    {
        private DriveService driveService;

        //GoogleAuthorizer is used to complete authentication and authorization
        private GoogleAuthorizer authorizer;
        public GoogleAuthorizer Authorizer { get { return authorizer; } }

        //DriveDownloader is used to content data to local
        private DriveDownloader downloader;
        public DriveDownloader Donwloader { get { return getDownloader(); } }

        //Google RESTful API base uri
        public static string BASE_URI = "https://www.googleapis.com/drive/v2/files/";
        //Folder to store downloaded files
        public string FOLDER_PATH;

        //Empty default constructor
        private DriveScanner(string path)
        {
            FOLDER_PATH = path;
            downloader = null;
        }

        //Use factory pattern to build and initialize object.
        //Because async method can not be used in constructor.
        private async Task<DriveScanner> InitializeAsync()
        {
            authorizer = new GoogleAuthorizer();
            driveService = await authorizer.AuthorizeAndGetDriveService();
            return this;
        }
        //Static method to create and return DriveScanner object
        public static async Task<DriveScanner>  CreateDriveScannerAysnc(string path)
        {
            DriveScanner analyzer = new DriveScanner(path);
            return await analyzer.InitializeAsync();
        }

        //Return a single instance of DriveDownloader
        private DriveDownloader getDownloader()
        {
            if (downloader == null)
                return new DriveDownloader(this);
            else
                return downloader;
        }


        
        //Store all files in IList
        public async Task<FileList> getFileListAsync()
        {
            FileList fileList = await driveService.Files.List().ExecuteAsync();
            return fileList;
        }
        //Print file list
        public async Task printFileListAsync()
        {
            FileList fileList = await driveService.Files.List().ExecuteAsync();
            int count = 1;

            foreach(Google.Apis.Drive.v2.Data.File entry in fileList.Items)
            {
                Console.WriteLine("{0}. {1}", count++, entry.Title);
            }
        }

        //Get File resource via file ID
        public async Task<Google.Apis.Drive.v2.Data.File> getFileEntryAsync(string fileId)
        {
            try
            {
                return await driveService.Files.Get(fileId).ExecuteAsync();
            }
            catch (Exception e)
            {
                throw new FileNotFoundException();
            }
        }


        //Download metadata of a file as stream
        public async Task<Stream> GetMetaDataStreamAsync(Google.Apis.Drive.v2.Data.File fileEntry)
        {
            //Generate URI to file resource
            Uri fileUri = new Uri(BASE_URI + fileEntry.Id);

            try
            {
                //Use HTTP client in DriveService to obtain response
                Task<Stream> fileJsonStream = driveService.HttpClient.GetStreamAsync(fileUri);
                Console.WriteLine("Downloading JSON file of {0}...", fileEntry.Title);

                return await fileJsonStream;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while downloading JSON file: " + e.Message);
                return null;
            }
        }



        //Download content of a file as stream
        public async Task<Stream> GetContentStreamAsync(Google.Apis.Drive.v2.Data.File fileEntry)
        {
            //Not every file resources in Google Drive has file content.
            //For example, a folder
            if (fileEntry.FileSize == null)
            {
                Console.WriteLine(fileEntry.Title + " is not a file. Skipped.");
                Console.WriteLine();
                return null;
            }

            //Generate URI to file resource
            //"alt=media" indicates downloading file content instead of JSON metadata
            Uri fileUri = new Uri(BASE_URI + fileEntry.Id + "?alt=media");

            try
            {
                //Use HTTP client in DriveService to obtain response
                Task<Stream> fileContentStream = driveService.HttpClient.GetStreamAsync(fileUri);
                Console.WriteLine("Downloading file {0}...", fileEntry.Title);

                return await fileContentStream;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while downloading file: " + e.Message);
                return null;
            }
        }



        //List a file's revisions based on file ID
        public async Task<IList<Revision>> getRevisionsAsync(string fileID)
        {
            var revisionList = await driveService.Revisions.List(fileID).ExecuteAsync();
            return revisionList.Items;
        }
        //Print the number of each file's revisions
        public async Task<List<RevisionViewModel>> listRevisionsAsync()
        {
            List<RevisionViewModel> revisionsList = new List<RevisionViewModel>();
            var entryList = (await getFileListAsync()).Items;
            foreach(var entry in entryList)
            {
                int count = (await getRevisionsAsync(entry.Id)).Count;
                revisionsList.Add(new RevisionViewModel(entry, count));
            }
            return revisionsList;
        }
        //Download a revision as stream
        public async Task<Stream> GetRevisionStreamAsync(string fileID, string revisionID)
        {
            Uri revisionUrl = new Uri(BASE_URI + fileID + "/revisions/" + revisionID);

            try
            {
                //Use HTTP client in DriveService to obtain response
                Task<Stream> revisionStream = driveService.HttpClient.GetStreamAsync(revisionUrl);
                Console.WriteLine("Downloading revision {0}...", revisionID);

                return await revisionStream;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while downloading revisions: " + e.Message);
                return null;
            }
        }



        //Used when delegate is an async method
        public async Task ParrallelProcessAsync(ProcessFileAsync process)
        {
            var fileList = await driveService.Files.List().ExecuteAsync();

            if (fileList.Items == null)
            {
                Console.WriteLine("No file is found.");
                return;
            }

            var allFiles = fileList.Items;

            foreach (Google.Apis.Drive.v2.Data.File fileResource in allFiles)
            {
                await process(fileResource);
            }
        }
        //Used when delegate is a blocking method
        public async Task BlockingProcessAsync(ProcessFile process)
        {
            var fileList = await driveService.Files.List().ExecuteAsync();

            if (fileList.Items == null)
            {
                Console.WriteLine("No file is found.");
                return;
            }

            var allFiles = fileList.Items;

            foreach (Google.Apis.Drive.v2.Data.File fileResource in allFiles)
            {
                process(fileResource);
            }
        }



        //Delete stored token
        public async Task ClearTokens()
        {
            await Authorizer.ClearCredential();
        }
    }

    //Delegate for async methods which accept File as parameter and return void
    public delegate Task ProcessFileAsync(Google.Apis.Drive.v2.Data.File fileResource);

    //Delegate for blocking methods which accept File as parameter and return void
    public delegate void ProcessFile(Google.Apis.Drive.v2.Data.File fileResource);
}
