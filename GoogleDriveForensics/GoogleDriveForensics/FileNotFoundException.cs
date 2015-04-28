using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDriveForensics
{
    class FileNotFoundException : ApplicationException
    {
        public FileNotFoundException() { }

        public override string Message
        {
            get
            {
                return "The requested file does not exist";
            }
        }
    }
}
