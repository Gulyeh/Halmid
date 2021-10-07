using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Halmid_Client.Variables
{
    class UserData
    {
        public static string Name { get; set; }
        public static string ChannelID { get; set; }
        public static string LoginID { get; set; }
        public static string Username { get; set; }
        public static BitmapImage Avatar { get; set; }
        public static string Status { get; set; }
    }
    public class MessageDataView
    {
        public string MessageID { get; set; }
        public string Content { get; set; }
        public string From { get; set; }
        public string Timestamp { get; set; }
        public string Colored { get; set; }
        public BitmapImage Avatar { get; set; }
        public string Sender_id { get; set; }
        public string ImageID { get; set; }
        public BitmapImage ImageSource { get; set; }
    }
    public class MessageSender_Handler
    {
        public string userID { get; set; }
        public BitmapImage Avatar { get; set; }
    }
    public class WritingPersons
    {
        public string Name { get; set; }
        public string userID { get; set; }
        public string ChannelHash { get; set; }
    }
    public class ChannelList : INotifyPropertyChanged
    {
        private string name { get; set; }
        public string Name
        {
            get { return name; }
            set 
            {
                if (name == value) return;
                name = value;
                NotifyPropertyChanged("Name");
            }
        }
        private string channelHash { get; set; }
        public string ChannelHash
        {
            get { return channelHash; }
            set
            {
                if (channelHash == value) return;
                channelHash = value;
                NotifyPropertyChanged("ChannelHash");
            }
        }
        private BitmapImage avatar { get; set; }
        public BitmapImage Avatar
        {
            get { return avatar; }
            set
            {
                if (avatar == value) return;
                avatar = value;
                NotifyPropertyChanged("Avatar");
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class _ChannelList
    {
        public static ObservableCollection<ChannelList> Channels = new ObservableCollection<ChannelList>();
    }
    public class Online_Users_inChannel : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (name == value) return;
                name = value;
                NotifyPropertyChanged("Name");
            }
        }
        private string _userID;
        public string userID
        {
            get { return _userID; }
            set
            {
                if (_userID == value) return;
                _userID = value;
                NotifyPropertyChanged("userID");
            }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set
            {
                if (status == value) return;
                status = value;
                NotifyPropertyChanged("Status");
            }
        }

        private BitmapImage avatar;
        public BitmapImage Avatar
        {
            get { return avatar; }
            set
            {
                if (avatar == value) return;
                avatar = value;
                NotifyPropertyChanged("Avatar");
            }
        }

        private string _isAdmin;
        public string isAdmin
        {
            get { return _isAdmin; }
            set
            {
                if (_isAdmin == value) return;
                _isAdmin = value;
                NotifyPropertyChanged("isAdmin");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class Private_Users
    {
        public string Name { get; set; }
        public string userID { get; set; }
        public string Status { get; set; }
        public string channelID { get; set; }
        public BitmapImage Avatar { get; set; }
        public string isBlocked { get; set; }
    }
    public class _Friends
    {
        public static List<Online_Users_inChannel> friends = new List<Online_Users_inChannel>();
    }
    public class _Pending
    {
        public static List<Online_Users_inChannel> pending = new List<Online_Users_inChannel>();
    }
    public class _Private_Users
    {
        public static ObservableCollection<Private_Users> PrivateUsers_Data = new ObservableCollection<Private_Users>();
    }
}
