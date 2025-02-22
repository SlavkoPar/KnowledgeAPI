using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Knowledge.Model
{

    public class WhoWhen
    {
        DateTime date { get; set; }
        string nickName { get; set; }
    }
    public class Record
    {
        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }
    }
}

