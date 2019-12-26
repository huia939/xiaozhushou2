using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entity.WebApiResponse
{
    public class UserLoginResponse : DefaultReponse
    {
        public UserLoginResult result
        {
            get; set;
        }

        public class UserLoginResult
        {
            public string token
            {
                get; set;
            }

            //public User user
            //{
            //    get; set;
            //}
        }

        //public class User
        //{
        //    public string token
        //    {
        //        get; set;
        //    }
        //}
    }
}
