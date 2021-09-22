using Halmid_Client.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Halmid_Client.Chat_Functions
{
    class WritingMessage_Notification
    {
        public static List<WritingPersons> WritingUsersData = new List<WritingPersons>();
        public static string WritingMessage_Channel(string name, string roomid, bool isWriting, string userID)
        {
            if (isWriting == true)
            {
                WritingPersons writingUser = new WritingPersons();
                writingUser.Name = name;
                writingUser.ChannelHash = roomid;
                writingUser.userID = userID;
                WritingUsersData.Add(writingUser);
            }
            else
            {
                WritingUsersData.RemoveAll(x => x.userID == userID && x.ChannelHash == roomid);
            }

            if (WritingUsersData.Count(x => x.ChannelHash == roomid) > 4)
            {
                return "Multiple people are typing...";
            }
            else if (WritingUsersData.Count(x => x.ChannelHash == roomid) > 0)
            {
                if (WritingUsersData.Count(x => x.ChannelHash == roomid) == 1)
                {
                    return name + " is typing...";
                }
                else
                {
                    string data = String.Empty;
                    foreach (WritingPersons user in WritingUsersData.FindAll(x => x.ChannelHash == roomid))
                    {
                        data += user.Name + ", ";
                    }
                    return data.Substring(0, data.Length - 2) + " are typing...";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
