using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Halmid_Server.Hubs
{
    public class User
    {
        public string Name { get; set; }
        public string ChannelID { get; set; }
        public string LoginID { get; set; }
        public string AvatarID { get; set; }
    }
    public class Online_Users_inChannel
    {
        public string Name { get; set; }
        public string userID { get; set; }
        public string Status { get; set; }
        public string isAdmin { get; set; }
    }
    public class Private_Users
    {
        public string Name { get; set; }
        public string userID { get; set; }
        public string Status { get; set; }
        public string channelID { get; set; }
        public string Avatar { get; set; }
        public string isBlocked { get; set; }
    }
    public class Connection_Handle
    {
        public static Dictionary<string, string> userConnectionids = new Dictionary<string, string>();
    }

}
