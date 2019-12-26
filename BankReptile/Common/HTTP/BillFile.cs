using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.HTTP
{
   public class BillFile
    {
        public DateTime StartDate
        {
            get;
            set;
        }

        public DateTime EndDate
        {
            get;
            set;
        }

        public string FullFilePath
        {
            get;
            set;
        }
    }
}
