using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Apis.Drive.v2.Data;

namespace GoogleDriveForensics
{
    public class RevisionViewModel
    {
        private string entryID;
        private string entryTitile;
        private int entryRevisionCount;

        public string EntryID { get { return entryID; } }
        public string EntryTitle { get { return entryTitile; } }
        public int EntryRevisionCount { get { return entryRevisionCount; } }

        public RevisionViewModel(File entry, int count)
        {
            entryID = entry.Id;
            entryTitile = entry.Title;
            entryRevisionCount = count;
        }
    }
}
