using System.Windows.Media.Imaging;

namespace Halmid_Server.Hubs
{
    public class Channels
    {
        public string Name { get; set; }
        public string ChannelHash { get; set; }
        public string Avatar { get; set; }
    }
    public class Channel_Messages
    {
        public string MessageID { get; set; }
        public string Content { get; set; }
        public string From { get; set; }
        public string Timestamp { get; set; }
        public string Avatar { get; set; }
        public string Colored { get; set; }
        public string Sender_id { get; set; }
        public string ImageID { get; set; }
    }
}
