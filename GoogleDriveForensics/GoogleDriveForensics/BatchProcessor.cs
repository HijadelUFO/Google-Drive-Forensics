using Google.Apis.Drive.v2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveForensics
{
    //Delegate for async methods which accept File as parameter and return void
    public delegate Task ProcessFileAsync(Google.Apis.Drive.v2.Data.File fileResource);
    
    //Delegate for blocking methods which accept File as parameter and return void
    public delegate void ProcessFile(Google.Apis.Drive.v2.Data.File fileResource);

    public class BatchProcessor
    {
        public BatchProcessor(DriveService driServ)
        {
            driveService = driServ;
        }

        //Used when delegate is an async method
        public async Task BatchProcessAsync(ProcessFileAsync process)
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
        public async Task BatchListOnlyAsync(ProcessFile process)
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

        private DriveService driveService;
    }
}
