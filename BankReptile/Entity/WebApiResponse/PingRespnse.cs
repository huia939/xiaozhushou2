using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity.WebApiResponse
{
    public class PingRespnse : DefaultReponse
    {
        public PingResult result
        {
            get; set;
        }

        public class PingResult
        {
            public int page
            {
                get; set;
            }

            public int need_data
            {
                get; set;
            }
        }
    }
}
