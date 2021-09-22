﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ChannelList
    {
        public string Name { get; set; }
        public string ChannelHash { get; set; }
        public BitmapImage Avatar { get; set; }
    }
    public class _ChannelList
    {
        public static ObservableCollection<ChannelList> Channels = new ObservableCollection<ChannelList>();
    }
    public class Online_Users_inChannel
    {
        public string Name { get; set; }
        public string userID { get; set; }
        public string Status { get; set; }
        public BitmapImage Avatar {get; set;}
        public string isAdmin { get; set; }
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
