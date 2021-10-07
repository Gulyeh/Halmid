using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Halmid_Client.Variables
{
    class Global_Variables
    {
        public static readonly string Version = "1.28";
        public static string Last_userID_onlineMenu { get; set; }
        public static Border last_clickedChannel_Border { get; set; }
        public static string channelType { get; set; }
        public static string status = "online";
        public static string api_login { get; set; }
        public static string api_pass { get; set; }
        public static string api_accessurl { get; set; }
        public static string api_uploadurl { get; set; }
    }
}
