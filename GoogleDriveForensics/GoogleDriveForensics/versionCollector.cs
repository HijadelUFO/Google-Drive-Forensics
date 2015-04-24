using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveForensics
{
    class versionCollector
    {
        private DriveService driveService;

        public versionCollector(DriveService service)
        {
            driveService = service;
        }

        public async Task<IList<Revision>> getRevisionsAsync(string fileID)
        {
            var revisionList = await driveService.Revisions.List(fileID).ExecuteAsync();
            return revisionList.Items;
        }
    }
}
