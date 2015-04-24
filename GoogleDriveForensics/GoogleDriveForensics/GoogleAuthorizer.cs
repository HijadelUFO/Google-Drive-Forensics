using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleDriveForensics
{
    public class GoogleAuthorizer
    {
        private readonly IDataStore credStore;

        //public IDataStore DataStore { get { return dataStore; } }

        public GoogleAuthorizer(string localFolderName = "GoogleDriveForensics")
        {
            credStore = new FileDataStore(localFolderName);
        }

        public async Task<DriveService> AuthorizeAndGetDriveService()
        {
            UserCredential credential;

            using (var stream = new System.IO.FileStream("client.json",
                System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { DriveService.Scope.DriveReadonly },
                    "user", CancellationToken.None, credStore);
            }

            // Create the service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google Drive Forensics",
            });

            Console.WriteLine("Google Drvie connected.");
            Console.WriteLine();

            return service;
        }

        //Clear tokens stored in local file
        public async Task ClearCredential()
        {
            await credStore.ClearAsync();
        }
    }
}
