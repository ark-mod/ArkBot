using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class Constants : IConstants
    {
        public string DatabaseConnectionString => "type=embedded;storesdirectory=.\\Database;storename=Default";
        public string OpenidresponsetemplatePath => @"Resources\openidresponse.html";
    }
}
