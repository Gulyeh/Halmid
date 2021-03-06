using Halmid_Server.Functions;
using Halmid_Server.WebApi_Data;
using Microsoft.AspNetCore.SignalR;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Halmid_Server.Hubs
{
    public class ChatHub : Hub
    {

        private readonly IDictionary<string, User> _connectionInfo;

        public ChatHub(IDictionary<string, User> conn)
        {
            _connectionInfo = conn;
        }

        //Client functions
        public async Task EnterKey(string key, string connID)
        {
            bool accepted = key == "5ecc578f1254c47de84eb375eb292d5818515cb95b01e17f499b257214066d4c" ? true : false;
            await Clients.Client(connID).SendAsync("CheckKey", accepted, CheckUpdate.Version);
        }
        public async Task Reconnect(string loginid, string status, string channelid)
        {
            try
            {
                LoginToAccount(loginid, status, channelid);

                if (channelid != null && channelid != String.Empty)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, channelid);
                }
            }
            catch (Exception) { }
        }
        public async Task LoginAccount(string loginid, string login, string status)
        {
            try
            {
                using (MySqlCommand cmd = Startup.connection.CreateCommand())
                {
                    Dictionary<string, string> apiaccess = new Dictionary<string, string>
                    {
                        {"login", "balistic"},
                        {"pass", "airplane" },
                        {"access_url", "http://31.178.21.16:7345/api/Security/GetToken" },
                        {"upload_url", "http://31.178.21.16:7345/api/UploadImage" }
                    };

                    try
                    {
                        cmd.CommandText = String.Format("SELECT login FROM users WHERE loginid = '{0}'", loginid);
                        string logindb = cmd.ExecuteScalar().ToString();

                        if (logindb != login)
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("LoginStatus", "wrong_login", "");
                        }
                        else
                        {
                            LoginToAccount(loginid, status, "");
                            if (status == "offline")
                            {
                                status = "Gray";
                            }
                            else
                            {
                                status = status == "online" ? "Green" : "Yellow";
                            }
                            await Clients.Client(Context.ConnectionId).SendAsync("LoginStatus", "logged", status, apiaccess);
                        }
                    }
                    catch (Exception)
                    {
                        cmd.CommandText = String.Format("INSERT INTO users (loginid, login, name, status, avatar) VALUES('{0}', '{1}', '{2}', '{3}', '{4}')", loginid, login, "Halmid_User", "online", "default");
                        cmd.ExecuteScalar();
                        Directory.CreateDirectory("C:/xampp/htdocs/Users/" + loginid);
                        User userData = new User();
                        userData.Name = "Halmid_User";
                        userData.ChannelID = "";
                        userData.LoginID = loginid;
                        userData.AvatarID = "default";

                        if (status == "offline")
                        {
                            status = "Gray";
                        }
                        else
                        {
                            status = status == "online" ? "Green" : "Yellow";
                        }

                        _connectionInfo[Context.ConnectionId] = userData;
                        Connection_Handle.userConnectionids.Add(loginid, Context.ConnectionId);

                        await Clients.Client(Context.ConnectionId).SendAsync("LoginStatus", "logged", status, apiaccess);
                    }
                }
            }
            catch (Exception)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("LoginStatus", "db_offline", "");
            }
        }
        public async Task Join_Channel(string Channel_ID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                string channelname = String.Empty;
                string avatar = String.Empty;
                string user_avatar = String.Empty;
                int isBanned = 0;

                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_name FROM channels WHERE channel_key = '{0}'", Channel_ID);
                        channelname = cmd.ExecuteScalar().ToString();

                        if (channelname != String.Empty && channelname != null)
                        {
                            cmd.CommandText = String.Format("SELECT COUNT(*) FROM bans WHERE banned_id = '{0}' AND channel_id = '{1}' AND ban_expire <= '{2}'", userData.LoginID, Channel_ID, DateTime.Now.ToString("dd/MM/yyyy H:mm"));
                            int banExpired = Int32.Parse(cmd.ExecuteScalar().ToString());

                            if (banExpired == 1)
                            {
                                cmd.CommandText = String.Format("DELETE FROM bans WHERE banned_id = '{0}' AND channel_id = '{1}'", userData.LoginID, Channel_ID);
                                cmd.ExecuteScalar();
                            }
                            else
                            {
                                cmd.CommandText = String.Format("SELECT COUNT(*) FROM bans WHERE banned_id = '{0}' AND channel_id = '{1}'", userData.LoginID, Channel_ID);
                                isBanned = Int32.Parse(cmd.ExecuteScalar().ToString());
                            }

                            if (isBanned == 0)
                            {
                                cmd.CommandText = String.Format("INSERT INTO users_in_channels (channel_id, user_id) VALUES('{0}', '{1}')", Channel_ID, userData.LoginID);
                                cmd.ExecuteScalar();

                                cmd.CommandText = String.Format("SELECT image FROM channels WHERE channel_key = '{0}'", Channel_ID);
                                using (MySqlDataReader readed = cmd.ExecuteReader())
                                {
                                    while (readed.Read())
                                    {
                                        avatar = ApiVariable.IPConnection + "Channels/" + readed.GetString("image") + ".png";
                                    }
                                    readed.Close();
                                }

                                await Clients.Client(Context.ConnectionId).SendAsync("Joined_Channel", true, channelname, Channel_ID, avatar);

                                Online_Users_inChannel user = new Online_Users_inChannel();
                                user.Name = userData.Name;
                                user.userID = userData.LoginID;
                                user.isAdmin = "Collapsed";
                                cmd.CommandText = String.Format("SELECT status FROM users WHERE loginid = '{0}'", userData.LoginID);
                                string status = cmd.ExecuteScalar().ToString();
                                if(status == "offline")
                                {
                                    user.Status = "Gray";
                                }
                                else
                                {
                                    user.Status = status == "online" ? "Green" : "Yellow";
                                }


                                if (userData.AvatarID != "default")
                                {
                                    user_avatar = ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + userData.AvatarID + ".png";
                                }
                                else
                                {
                                    user_avatar = ApiVariable.IPConnection + "Users/default.png";
                                }

                                await Clients.Group(Channel_ID).SendAsync("User_Online", user, user_avatar);

                                if (userData.ChannelID != String.Empty && userData.ChannelID != null)
                                {
                                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, userData.ChannelID);
                                }

                                await Groups.AddToGroupAsync(Context.ConnectionId, Channel_ID);
                                userData.ChannelID = Channel_ID;

                                Dictionary<string, string> msg = new Dictionary<string, string>
                                {
                                    {"message", $"{userData.Name} has joined!" }
                                };

                                await Clients.Group(Channel_ID).SendAsync("ReceiveMessage", "Server", msg, "-1", "-1", ApiVariable.IPConnection + "Users/default.png");
                            }
                            else
                            {
                                string reason = String.Empty;
                                string duration = String.Empty;
                                string admin_name = String.Empty;
                                string ban_expire = String.Empty;
                                string admin_id = String.Empty;

                                cmd.CommandText = String.Format("SELECT admin_id, reason, duration, ban_expire FROM bans WHERE banned_id = '{0}' AND channel_id = '{1}'", userData.LoginID, Channel_ID);
                                using (MySqlDataReader readed = cmd.ExecuteReader())
                                {
                                    while (readed.Read())
                                    {
                                        reason = readed.GetString("reason");
                                        duration = readed.GetString("duration");
                                        ban_expire = readed.GetString("ban_expire");
                                        admin_id = readed.GetString("admin_id");
                                    }
                                    readed.Close();
                                }
                                string converted_duration = Convert_Days.Convert(Int32.Parse(duration), ban_expire);
                                cmd.CommandText = String.Format("SELECT name FROM users WHERE loginid = '{0}'", admin_id);
                                admin_name = cmd.ExecuteScalar().ToString();
                                await Clients.Client(Context.ConnectionId).SendAsync("userBanned_fromChannel", admin_name, reason, converted_duration);
                            }
                        }
                        else
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("Joined_Channel", false, "", "", "");
                        }

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await Clients.Client(Context.ConnectionId).SendAsync("Joined_Channel", false, "", "", "");
                }
            }
        }
        public async Task SendMessage(Dictionary<string, string> message, string time, string type)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        if (message != null)
                        {
                            string messid = String.Empty;
                            bool Blocked = false;
                            string url;

                            if (type == "channel")
                            {
                                cmd.CommandText = String.Format("INSERT INTO messages_history (message, idsender, idchannel, time, decrypt_key) VALUES('{0}','{1}','{2}','{3}','{4}'); SELECT LAST_INSERT_ID();", message["message"], userData.LoginID, userData.ChannelID, time, message["key"]);
                                messid = cmd.ExecuteScalar().ToString();
                            }
                            else
                            {
                                cmd.CommandText = String.Format("SELECT Blocked_ID FROM blocked WHERE Channel_ID = '{0}'", userData.ChannelID);
                                using (MySqlDataReader readed = cmd.ExecuteReader())
                                {
                                    while (readed.Read())
                                    {
                                        if (readed.GetString("Blocked_ID") == userData.LoginID)
                                        {
                                            Blocked = true;
                                        }
                                    }
                                    readed.Close();
                                }

                                if (!Blocked)
                                {
                                    cmd.CommandText = String.Format("INSERT INTO messages_history (message, idsender, idchannel, time, decrypt_key) VALUES('{0}','{1}','{2}','{3}','{4}'); SELECT LAST_INSERT_ID();", message["message"], userData.LoginID, userData.ChannelID, time, message["key"]);
                                    messid = cmd.ExecuteScalar().ToString();
                                }
                                else
                                {
                                    Dictionary<string, string> msg = new Dictionary<string, string>
                                    {
                                        {"message", "Cannot send message.\nThis user has blocked you." }
                                    };

                                    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", "Server", msg, "-1", "-1", ApiVariable.IPConnection + "Users/default.png");
                                    return;
                                }
                            }

                            if (userData.AvatarID != "default")
                            {
                                url = ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + userData.AvatarID + ".png";
                            }
                            else
                            {
                                url = ApiVariable.IPConnection + "Users/default.png";
                            }

                            await Clients.Group(userData.ChannelID).SendAsync("ReceiveMessage", userData.Name, message, messid, userData.LoginID, url);
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }
        public async Task Send_ImageMessage(string imageid, Dictionary<string, string> message, string time, string type)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        string messid = String.Empty;
                        bool Blocked = false;
                        string url;

                        if (message != null)
                        {
                            if (type == "channel")
                            {
                                cmd.CommandText = String.Format("INSERT INTO messages_history (message, idsender, idchannel, time, image, decrypt_key) VALUES('{0}','{1}','{2}','{3}','{4}','{5}'); SELECT LAST_INSERT_ID();", message["message"], userData.LoginID, userData.ChannelID, time, imageid, message["key"]);
                                messid = cmd.ExecuteScalar().ToString();
                            }
                            else
                            {
                                cmd.CommandText = String.Format("SELECT Blocked_ID FROM blocked WHERE Channel_ID = '{0}'", userData.ChannelID);
                                using (MySqlDataReader readed = cmd.ExecuteReader())
                                {
                                    while (readed.Read())
                                    {
                                        if (readed.GetString("Blocked_ID") == userData.LoginID)
                                        {
                                            Blocked = true;
                                        }
                                    }
                                    readed.Close();
                                }
                                if (!Blocked)
                                {
                                    cmd.CommandText = String.Format("INSERT INTO messages_history (message, idsender, idchannel, time, image, decrypt_key) VALUES('{0}','{1}','{2}','{3}','{4}','{5}'); SELECT LAST_INSERT_ID();", message["message"], userData.LoginID, userData.ChannelID, time, imageid, message["key"]);
                                    messid = cmd.ExecuteScalar().ToString();
                                }
                                else
                                {
                                    Dictionary<string, string> msg = new Dictionary<string, string>
                                    {
                                        {"message", "Cannot send message.\nThis user has blocked you." }
                                    };

                                    await Clients.Client(Context.ConnectionId).SendAsync("Receive_ImageMessage", "Server", msg, "-1", "-1", ApiVariable.IPConnection + "Users/default.png");
                                    return;
                                }
                            }

                            if (userData.AvatarID != "default")
                            {
                                url = ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + userData.AvatarID + ".png";
                            }
                            else
                            {
                                url = ApiVariable.IPConnection + "Users/default.png";
                            }

                            await Clients.Group(userData.ChannelID).SendAsync("Receive_ImageMessage", userData.Name, message, ApiVariable.IPConnection + "Channels/" + userData.ChannelID + "/" + imageid + ".png", messid, userData.LoginID, url);
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                List<string> Channel_ids = new List<string>();
                List<string> Friends_ids = new List<string>();
                List<string> Privates_ids = new List<string>();

                using (MySqlCommand cmd = Startup.connection.CreateCommand())
                {
                    cmd.CommandText = String.Format("UPDATE users SET status = '{0}' WHERE loginid = '{1}'", "offline", userData.LoginID);
                    cmd.ExecuteScalar();

                    cmd.CommandText = String.Format("SELECT channel_id FROM users_in_channels WHERE user_id = '{0}'", userData.LoginID);
                    using (MySqlDataReader readed = cmd.ExecuteReader())
                    {
                        while (readed.Read())
                        {
                            Channel_ids.Add(readed.GetString("channel_id"));
                        }
                        readed.Close();
                    }

                    cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM friends WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                    using (MySqlDataReader readed = cmd.ExecuteReader())
                    {
                        while (readed.Read())
                        {
                            if (readed.GetString("user1_id") != userData.LoginID)
                            {
                                Friends_ids.Add(readed.GetString("user1_id"));
                            }
                            else if (readed.GetString("user2_id") != userData.LoginID)
                            {
                                Friends_ids.Add(readed.GetString("user2_id"));
                            }
                        }
                        readed.Close();
                    }

                    cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM private_channels WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                    using (MySqlDataReader readed = cmd.ExecuteReader())
                    {
                        while (readed.Read())
                        {
                            if (readed.GetString("user1_id") != userData.LoginID)
                            {
                                Privates_ids.Add(readed.GetString("user1_id"));
                            }
                            else if (readed.GetString("user2_id") != userData.LoginID)
                            {
                                Privates_ids.Add(readed.GetString("user2_id"));
                            }
                        }
                        readed.Close();
                    }
                }

                Online_Users_inChannel user = new Online_Users_inChannel();
                user.userID = userData.LoginID;
                user.Status = "Gray";

                foreach (string id in Friends_ids)
                {
                    if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                    {
                        await Clients.Client(ConnectionID).SendAsync("Friend_changedStatus", user);
                    }
                }

                foreach (string id in Channel_ids)
                {
                    await Clients.Group(id).SendAsync("User_Offline", user);
                }

                foreach (string id in Privates_ids)
                {
                    if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                    {
                        await Clients.Client(ConnectionID).SendAsync("Private_changeStatus", user);
                    }
                }

                if (userData.ChannelID != null && userData.ChannelID != String.Empty)
                {
                    await Clients.Group(userData.ChannelID).SendAsync("WritingMessage_Channel", userData.Name, userData.ChannelID, false, userData.LoginID);
                }

                Connection_Handle.userConnectionids.Remove(userData.LoginID);
                _connectionInfo.Remove(Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task WritingMessage(bool isWriting)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                await Clients.Group(userData.ChannelID).SendAsync("WritingMessage_Channel", userData.Name, userData.ChannelID, isWriting, userData.LoginID);
            }
        }
        public async Task Get_UserData()
        {
            try
            {
                if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
                {
                    string name = String.Empty;
                    string avatar = String.Empty;
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT name, avatar FROM users WHERE loginid = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                name = readed.GetString("name");
                                if(userData.AvatarID == "default")
                                {
                                    avatar = ApiVariable.IPConnection + "Users/default.png";
                                }
                                else
                                {
                                    avatar = ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + readed.GetString("avatar") + ".png";
                                }

                            }
                            readed.Close();
                        }
                    }
                    await Clients.Client(Context.ConnectionId).SendAsync("Receive_UserData", name, avatar);
                }
            }
            catch (Exception) { }
        }
        public async Task Get_UserPrivates()
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    List<Private_Users> Private_Data = new List<Private_Users>();
                    List<string> channels_id = new List<string>();
                    List<string> users_id = new List<string>();
                    List<string> avatar_links = new List<string>();
                    int i = 0;

                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channelID, user1_id, user2_id FROM private_channels WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                channels_id.Add(readed.GetString("channelID"));
                                if (readed.GetString("user1_id") != userData.LoginID)
                                {
                                    users_id.Add(readed.GetString("user1_id"));
                                }
                                else if (readed.GetString("user2_id") != userData.LoginID)
                                {
                                    users_id.Add(readed.GetString("user2_id"));
                                }
                            }
                            readed.Close();
                        }

                        foreach (string id in users_id)
                        {
                            Private_Users data = new Private_Users();

                            cmd.CommandText = String.Format("SELECT name, status, avatar FROM users WHERE loginid = '{0}'", id);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    data.Name = readed.GetString("name");
                                    data.userID = id;
                                    if (readed.GetString("status") == "offline")
                                    {
                                        data.Status = "Gray";
                                    }
                                    else
                                    {
                                        data.Status = readed.GetString("status") == "online" ? "Green" : "Yellow";
                                    }
                                    data.channelID = channels_id[i];
                                    if (readed.GetString("avatar") != "default")
                                    {
                                        avatar_links.Add(ApiVariable.IPConnection + "Users/" + id + "/" + readed.GetString("avatar") + ".png");
                                    }
                                    else
                                    {
                                        avatar_links.Add(ApiVariable.IPConnection + "Users/default.png");
                                    }
                                }
                                readed.Close();
                            }

                            cmd.CommandText = String.Format("SELECT COUNT(*) FROM blocked WHERE Channel_ID = '{0}' AND  Blocker_ID = '{1}'", channels_id[i], userData.LoginID);
                            int found = Int32.Parse(cmd.ExecuteScalar().ToString());
                            data.isBlocked = found == 1 ? "Unblock User" : "Block User";

                            Private_Data.Add(data);
                            i++;
                        }

                        await Clients.Client(Context.ConnectionId).SendAsync("Got_UserPrivates", Private_Data, avatar_links);
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Create_Channel(string channel_name, string avatarID)
        {
            try
            {
                if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        Guid id = Guid.NewGuid();

                        cmd.CommandText = String.Format("INSERT INTO channels (channel_key, channel_name, channel_admin, image) VALUES('{0}', '{1}', '{2}', '{3}')", id, channel_name, userData.LoginID, avatarID);
                        cmd.ExecuteScalar();

                        cmd.CommandText = String.Format("INSERT INTO users_in_channels (channel_id, user_id) VALUES('{0}', '{1}')", id, userData.LoginID);
                        cmd.ExecuteScalar();

                        Directory.CreateDirectory("C:/xampp/htdocs/Channels/" + id);

                        await Clients.Client(Context.ConnectionId).SendAsync("Created_Channel", true, id, channel_name, ApiVariable.IPConnection + "Channels/" + avatarID + ".png");
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
        }
        public async Task Get_UserChannels()
        {
            try
            {
                if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
                {
                    List<Channels> _Channels = new List<Channels>();
                    List<string> avatar_url = new List<string>();

                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = String.Format("SELECT channel_key, channel_name, image FROM channels WHERE channel_key IN (SELECT channel_id FROM users_in_channels WHERE user_id = '{0}')", userData.LoginID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    Channels data = new Channels();
                                    data.ChannelHash = readed.GetString("channel_key");
                                    data.Name = readed.GetString("channel_name");
                                    avatar_url.Add(ApiVariable.IPConnection + "Channels/" + readed.GetString("image") + ".png");
                                    _Channels.Add(data);
                                }
                                readed.Close();
                            }

                            await Clients.Client(Context.ConnectionId).SendAsync("Receive_UserChannels", _Channels, avatar_url);
                            _Channels.Clear();
                        }
                        catch (Exception)
                        { }
                    }

                }
            }
            catch (Exception) { }
        }
        public async Task Get_UserFriends()
        {
            try
            {
                if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
                {
                    List<Online_Users_inChannel> _Friends = new List<Online_Users_inChannel>();
                    List<Online_Users_inChannel> _Pending = new List<Online_Users_inChannel>();
                    List<string> _Friends_ID = new List<string>();
                    List<string> _Pending_ID = new List<string>();
                    List<string> avatar_links = new List<string>();

                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        try
                        {
                            cmd.CommandText = String.Format("SELECT user1_id, user2_id, status FROM friends WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    if (readed.GetString("status") == "Friends")
                                    {
                                        if (readed.GetString("user1_id") != userData.LoginID)
                                        {
                                            _Friends_ID.Add(readed.GetString("user1_id"));
                                        }
                                        else if (readed.GetString("user2_id") != userData.LoginID)
                                        {
                                            _Friends_ID.Add(readed.GetString("user2_id"));
                                        }
                                    }
                                    else
                                    {
                                        if (readed.GetString("user1_id") != userData.LoginID)
                                        {
                                            _Pending_ID.Add(readed.GetString("user1_id"));
                                        }
                                        else if (readed.GetString("user2_id") != userData.LoginID)
                                        {
                                            _Pending_ID.Add(readed.GetString("user2_id"));
                                        }
                                    }
                                }
                                readed.Close();
                            }

                            foreach (string id in _Friends_ID)
                            {
                                cmd.CommandText = String.Format("SELECT name, status, avatar FROM users WHERE loginid = '{0}'", id);
                                using (MySqlDataReader readed = cmd.ExecuteReader())
                                {
                                    while (readed.Read())
                                    {
                                        Online_Users_inChannel user = new Online_Users_inChannel();
                                        user.userID = id;
                                        user.Name = readed.GetString("name");
                                        if (readed.GetString("status") == "offline")
                                        {
                                            user.Status = "Gray";
                                        }
                                        else
                                        {
                                            user.Status = readed.GetString("status") == "online" ? "Green" : "Yellow";
                                        }
                                        if (readed.GetString("avatar") != "default")
                                        {
                                            avatar_links.Add(ApiVariable.IPConnection + "Users/" + id + "/" + readed.GetString("avatar") + ".png");
                                        }
                                        else
                                        {
                                            avatar_links.Add(ApiVariable.IPConnection + "Users/default.png");
                                        }
                                        _Friends.Add(user);
                                    }
                                    readed.Close();
                                }
                            }

                            await Clients.Client(Context.ConnectionId).SendAsync("get_Friends", _Friends, avatar_links);
                            avatar_links.Clear();

                            foreach (string id in _Pending_ID)
                            {
                                cmd.CommandText = String.Format("SELECT name, avatar FROM users WHERE loginid = '{0}'", id);
                                using (MySqlDataReader readed = cmd.ExecuteReader())
                                {
                                    while (readed.Read())
                                    {
                                        Online_Users_inChannel user = new Online_Users_inChannel();
                                        user.userID = id;
                                        user.Name = readed.GetString("name");
                                        if (readed.GetString("avatar") != "default")
                                        {
                                            avatar_links.Add(ApiVariable.IPConnection + "Users/" + id + "/" + readed.GetString("avatar") + ".png");
                                        }
                                        else
                                        {
                                            avatar_links.Add(ApiVariable.IPConnection + "Users/default.png");
                                        }
                                        _Pending.Add(user);
                                    }
                                    readed.Close();
                                }
                            }

                            await Clients.Client(Context.ConnectionId).SendAsync("Pending_friendRequests", _Pending, avatar_links);
                        }
                        catch (Exception e)
                        { Console.WriteLine(e); }
                    }

                }
            }
            catch (Exception) { }
        }
        public async Task Switch_Channel(string channelID, string type)
        {
            try
            {
                if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
                {
                    Dictionary<string, string> crypted = new Dictionary<string, string>();
                    List<Channel_Messages> _Messages = new List<Channel_Messages>();
                    List<Online_Users_inChannel> Online_Users = new List<Online_Users_inChannel>();
                    List<string> avatar_links_msg = new List<string>();
                    List<string> avatar_links_online = new List<string>();

                    string admin = String.Empty;

                    if (userData.ChannelID != String.Empty && userData.ChannelID != null)
                    {
                        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userData.ChannelID);
                    }

                    userData.ChannelID = channelID;

                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT idmessages, message, time, name, idsender, image, avatar, loginid, decrypt_key FROM (SELECT idmessages, message, time, users.name, idsender, image, avatar, loginid, decrypt_key FROM messages_history, users WHERE idchannel='{0}' AND users.loginid = messages_history.idsender ORDER BY idmessages DESC LIMIT 100) sub ORDER BY idmessages ASC", channelID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            int i = 0;
                            while (readed.Read())
                            {
                                Channel_Messages msg = new Channel_Messages();
                                msg.MessageID = readed.GetString("idmessages");
                                msg.From = readed.GetString("name");
                                msg.Timestamp = readed.GetString("time");
                                msg.Sender_id = readed.GetString("idsender");
                                msg.Colored = userData.LoginID == readed.GetString("idsender") ? "DarkGreen" : "White";
                                if (!readed.IsDBNull(readed.GetOrdinal("message")) && !readed.IsDBNull(readed.GetOrdinal("decrypt_key")))
                                {
                                    crypted.Add("message" + i, readed.GetString("message"));
                                    crypted.Add("key" + i, readed.GetString("decrypt_key"));
                                }

                                if (!readed.IsDBNull(readed.GetOrdinal("image")))
                                {
                                    msg.ImageID = ApiVariable.IPConnection + "Channels/" + userData.ChannelID + "/" + readed.GetString("image") + ".png";
                                }

                                if (readed.GetString("avatar") != "default")
                                {
                                    avatar_links_msg.Add(ApiVariable.IPConnection + "Users/" + readed.GetString("idsender") + "/" + readed.GetString("avatar") + ".png");
                                }
                                else
                                {
                                    avatar_links_msg.Add(ApiVariable.IPConnection + "Users/default.png");
                                }
                                _Messages.Add(msg);
                                i++;
                            }
                            readed.Close();
                        }

                        if (type == "channel")
                        {
                            cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key = '{0}'", channelID);
                            admin = cmd.ExecuteScalar().ToString();

                            cmd.CommandText = String.Format("SELECT user_id, name, status, avatar FROM users_in_channels, users WHERE channel_id = '{0}' AND (status = 'online' OR status = 'away' ) AND user_id = loginid;", channelID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    Online_Users_inChannel online = new Online_Users_inChannel();
                                    online.Name = readed.GetString("name");
                                    online.userID = readed.GetString("user_id");
                                    online.Status = readed.GetString("status") == "online" ? "Green" : "Yellow";
                                    online.isAdmin = admin == readed.GetString("user_id") ? "Visible" : "Collapsed";
                                    if (readed.GetString("avatar") != "default")
                                    {
                                        avatar_links_online.Add(ApiVariable.IPConnection + "Users/" + readed.GetString("user_id") + "/" + readed.GetString("avatar") + ".png");
                                    }
                                    else
                                    {
                                        avatar_links_online.Add(ApiVariable.IPConnection + "Users/default.png");
                                    }
                                    Online_Users.Add(online);
                                }
                                readed.Close();
                            }

                            await Clients.Client(Context.ConnectionId).SendAsync("Switched_Channel", true, _Messages, crypted, Online_Users, admin, avatar_links_msg, avatar_links_online);
                        }
                        else
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("Switched_privateChannel", true, _Messages, crypted, channelID, avatar_links_msg);
                        }

                        await Groups.AddToGroupAsync(Context.ConnectionId, channelID);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
        }
        public async Task Check_channelAdmin()
        {
            try
            {
                if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key='{0}'", userData.ChannelID);
                        string admin_id = cmd.ExecuteScalar().ToString();

                        if (userData.LoginID == admin_id)
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("Check_channelAdmin_user", true);
                        }
                        else
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("Check_channelAdmin_user", false);
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        public async Task Change_user_Nick(string new_nick)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                List<string> Channel_ids = new List<string>();
                List<string> Friends_ids = new List<string>();
                List<string> Privates_ids = new List<string>();
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        if (userData.Name != new_nick)
                        {
                            cmd.CommandText = String.Format("UPDATE users SET name = '{0}' WHERE loginid = '{1}'", new_nick, userData.LoginID);
                            cmd.ExecuteScalar();

                            cmd.CommandText = String.Format("SELECT channel_id FROM users_in_channels WHERE user_id = '{0}';", userData.LoginID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    Channel_ids.Add(readed.GetString("channel_id"));
                                }
                                readed.Close();
                            }

                            cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM friends WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    if (readed.GetString("user1_id") != userData.LoginID)
                                    {
                                        Friends_ids.Add(readed.GetString("user1_id"));
                                    }
                                    else if (readed.GetString("user2_id") != userData.LoginID)
                                    {
                                        Friends_ids.Add(readed.GetString("user2_id"));
                                    }
                                }
                                readed.Close();
                            }

                            cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM private_channels WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    if (readed.GetString("user1_id") != userData.LoginID)
                                    {
                                        Privates_ids.Add(readed.GetString("user1_id"));
                                    }
                                    else if (readed.GetString("user2_id") != userData.LoginID)
                                    {
                                        Privates_ids.Add(readed.GetString("user2_id"));
                                    }
                                }
                                readed.Close();
                            }

                            userData.Name = new_nick;

                            foreach (string id in Friends_ids)
                            {
                                if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                                {
                                    await Clients.Client(ConnectionID).SendAsync("Friend_updatedName", userData.Name, userData.LoginID);
                                }
                            }

                            foreach (string id in Privates_ids)
                            {
                                if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                                {
                                    await Clients.Client(ConnectionID).SendAsync("User_Updated_Name", userData.Name, userData.LoginID);
                                }
                            }

                            foreach (string id in Channel_ids)
                            {
                                await Clients.Group(id).SendAsync("User_Updated_Name", userData.Name, userData.LoginID);
                            }



                            await Clients.Client(Context.ConnectionId).SendAsync("Update_Name", true, new_nick);

                            Channel_ids.Clear();
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Change_user_Login(string new_login)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("UPDATE users SET login = '{0}' WHERE loginid = '{1}'", new_login, userData.LoginID);
                        cmd.ExecuteScalar();
                        await Clients.Client(Context.ConnectionId).SendAsync("Updated_Login", true, new_login);
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Change_channel_Name(string new_channel_name)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key = '{0}'", userData.ChannelID);
                        string adminid = cmd.ExecuteScalar().ToString();

                        if (adminid == userData.LoginID)
                        {
                            List<string> users_in_channel = new List<string>();

                            cmd.CommandText = String.Format("UPDATE channels SET channel_name = '{0}' WHERE channel_key = '{1}'", new_channel_name, userData.ChannelID);
                            cmd.ExecuteScalar();

                            cmd.CommandText = String.Format("SELECT user_id FROM users_in_channels WHERE channel_id = '{0}'", userData.ChannelID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    users_in_channel.Add(readed.GetString("user_id"));
                                }
                                readed.Close();
                            }

                            foreach (string user in users_in_channel)
                            {
                                if (Connection_Handle.userConnectionids.TryGetValue(user, out string ConnectionID))
                                {
                                    await Clients.Client(ConnectionID).SendAsync("Updated_Channel_Name", true, new_channel_name, userData.ChannelID);
                                }
                            }

                            Dictionary<string, string> msg = new Dictionary<string, string>
                            {
                                {"message", "Admin changed server name to: " + new_channel_name }
                            };

                            await Clients.Group(userData.ChannelID).SendAsync("ReceiveMessage", "Server", msg, "-1", "-1", ApiVariable.IPConnection + "Users/default.png");

                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Change_userStatus(string status, string channel_Type)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                List<string> Channel_ids = new List<string>();
                List<string> Friends_ids = new List<string>();
                List<string> Privates_ids = new List<string>();

                List<Online_Users_inChannel> Online_Users = new List<Online_Users_inChannel>();
                string admin = String.Empty;

                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("UPDATE users SET status = '{0}' WHERE loginid = '{1}'", status, userData.LoginID);
                        cmd.ExecuteScalar();

                        if (userData.ChannelID != null && userData.ChannelID != String.Empty)
                        {
                            try
                            {
                                cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key = '{0}'", userData.ChannelID);
                                admin = cmd.ExecuteScalar().ToString();
                            }
                            catch (Exception) { }
                        }

                        cmd.CommandText = String.Format("SELECT channel_id FROM users_in_channels WHERE user_id = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                Channel_ids.Add(readed.GetString("channel_id"));
                            }
                            readed.Close();
                        }

                        cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM friends WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                if (readed.GetString("user1_id") != userData.LoginID)
                                {
                                    Friends_ids.Add(readed.GetString("user1_id"));
                                }
                                else if (readed.GetString("user2_id") != userData.LoginID)
                                {
                                    Friends_ids.Add(readed.GetString("user2_id"));
                                }
                            }
                            readed.Close();
                        }

                        cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM private_channels WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                if (readed.GetString("user1_id") != userData.LoginID)
                                {
                                    Privates_ids.Add(readed.GetString("user1_id"));
                                }
                                else if (readed.GetString("user2_id") != userData.LoginID)
                                {
                                    Privates_ids.Add(readed.GetString("user2_id"));
                                }
                            }
                            readed.Close();
                        }

                        Online_Users_inChannel online = new Online_Users_inChannel();
                        online.Name = userData.Name;
                        online.userID = userData.LoginID;
                        if (status == "offline")
                        {
                            online.Status = "Gray";
                        }
                        else
                        {
                            online.Status = status == "online" ? "Green" : "Yellow";
                        }
                        online.isAdmin = admin == userData.LoginID ? "Visible" : "Collapsed";
                        Online_Users.Add(online);

                        foreach (string id in Channel_ids)
                        {
                            await Clients.Group(id).SendAsync("Changed_userStatus", online, true, ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + userData.AvatarID + ".png");
                        }

                        foreach (string id in Privates_ids)
                        {
                            if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                            {
                                await Clients.Client(ConnectionID).SendAsync("Private_changeStatus", online);
                            }
                        }

                        foreach (string id in Friends_ids)
                        {
                            if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                            {
                                await Clients.Client(ConnectionID).SendAsync("Friend_changedStatus", online);
                            }
                        }

                        if (userData.ChannelID == null || userData.ChannelID == String.Empty || channel_Type == "private")
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("Changed_userStatus", online, true, ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + userData.AvatarID + ".png");
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }
        public async Task Leave_Channel()
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key = '{0}'", userData.ChannelID);
                        bool admin = cmd.ExecuteScalar().ToString() == userData.LoginID ? true : false;

                        if (!admin)
                        {
                            cmd.CommandText = String.Format("DELETE FROM users_in_channels WHERE user_id = '{0}' AND channel_id = '{1}'", userData.LoginID, userData.ChannelID);
                            cmd.ExecuteScalar();
                            await Clients.Group(userData.ChannelID).SendAsync("userLeft_Channel", true, userData.LoginID, userData.ChannelID);
                            if (userData.ChannelID != String.Empty && userData.ChannelID != null)
                            {
                                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userData.ChannelID);
                            }
                            await Clients.Group(userData.ChannelID).SendAsync("WritingMessage_Channel", userData.Name, userData.ChannelID, false, userData.LoginID);
                            
                            Dictionary<string, string> msg = new Dictionary<string, string>
                            {
                                {"message", userData.Name + " left the server." }
                            };
                            await Clients.Group(userData.ChannelID).SendAsync("ReceiveMessage", "Server", msg, "-1","-1", ApiVariable.IPConnection + "Users/default.png");
                            userData.ChannelID = String.Empty;
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Find_user(string name)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    List<Online_Users_inChannel> userlist = new List<Online_Users_inChannel>();
                    List<string> avatar_links = new List<string>();

                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT loginid, name, status, avatar FROM users WHERE name LIKE '%{0}%'", name);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                if (readed.GetString("loginid") != userData.LoginID)
                                {
                                    Online_Users_inChannel user = new Online_Users_inChannel();
                                    user.Name = readed.GetString("name");
                                    user.userID = readed.GetString("loginid");
                                    if (readed.GetString("status") == "offline")
                                    {
                                        user.Status = "Gray";
                                    }
                                    else
                                    {
                                        user.Status = readed.GetString("status") == "online" ? "Green" : "Yellow";
                                    }

                                    if (readed.GetString("avatar") != "default")
                                    {
                                        avatar_links.Add(ApiVariable.IPConnection + "Users/" + readed.GetString("loginid") + "/" + readed.GetString("avatar") + ".png");
                                    }
                                    else
                                    {
                                        avatar_links.Add(ApiVariable.IPConnection + "Users/default.png");
                                    }
                                    userlist.Add(user);
                                }
                            }
                            readed.Close();
                        }

                        await Clients.Client(Context.ConnectionId).SendAsync("foundSearch", userlist, userlist.Count, avatar_links);
                        userlist.Clear();
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Delete_Message(string messageid)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("DELETE FROM messages_history WHERE idmessages = '{0}'", messageid);
                        cmd.ExecuteScalar();
                        await Clients.Group(userData.ChannelID).SendAsync("Deleted_Message", true, messageid);
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Edit_Message(string messageid, Dictionary<string, string> message)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("UPDATE messages_history SET message = '{0}', edited = '{1}', decrypt_key = '{3}' WHERE idmessages = '{2}'", message["message"], 1, messageid, message["key"]);
                        cmd.ExecuteScalar();
                        await Clients.Group(userData.ChannelID).SendAsync("Edited_Message", true, messageid, message);
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Delete_Channel(string channelID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                ;
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key='{0}'", channelID);
                        string admin_id = cmd.ExecuteScalar().ToString();

                        if (userData.LoginID == admin_id)
                        {
                            List<string> users_in_channel = new List<string>();

                            cmd.CommandText = String.Format("SELECT user_id FROM users_in_channels WHERE channel_id = '{0}'", channelID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    users_in_channel.Add(readed.GetString("user_id"));
                                }
                                readed.Close();
                            }

                            cmd.CommandText = String.Format("DELETE FROM users_in_channels WHERE channel_id = '{0}'", channelID);
                            cmd.ExecuteScalar();

                            cmd.CommandText = String.Format("DELETE FROM channels WHERE channel_key = '{0}'", channelID);
                            cmd.ExecuteScalar();

                            cmd.CommandText = String.Format("DELETE FROM messages_history WHERE idchannel = '{0}'", channelID);
                            cmd.ExecuteScalar();

                            Directory.Delete("C:/xampp/htdocs/Channels/" + channelID);

                            foreach (string user in users_in_channel)
                            {
                                _connectionInfo.TryGetValue(Connection_Handle.userConnectionids[user], out User Channel_userData);
                                if (userData.ChannelID == channelID)
                                {
                                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, Channel_userData.ChannelID);
                                }
                                Channel_userData.ChannelID = null;
                                await Clients.Client(Connection_Handle.userConnectionids[user]).SendAsync("Deleted_Channel", true, channelID);
                            }

                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Kick_User(string userID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key='{0}'", userData.ChannelID);
                        string admin_id = cmd.ExecuteScalar().ToString();

                        if (userData.LoginID == admin_id)
                        {
                            cmd.CommandText = String.Format("DELETE FROM users_in_channels WHERE user_id = '{0}'", userID);
                            cmd.ExecuteScalar();

                            _connectionInfo.TryGetValue(Connection_Handle.userConnectionids[userID], out User Kicked_userData);

                            await Clients.Group(userData.ChannelID).SendAsync("userLeft_Channel", true, Kicked_userData.LoginID, userData.ChannelID);

                            if (Kicked_userData.ChannelID == userData.ChannelID)
                            {
                                await Clients.Group(userData.ChannelID).SendAsync("WritingMessage_Channel", Kicked_userData.Name, userData.ChannelID, false, Kicked_userData.LoginID);
                                await Groups.RemoveFromGroupAsync(Connection_Handle.userConnectionids[userID], Kicked_userData.ChannelID);
                                Kicked_userData.ChannelID = null;
                            }
                            else
                            {
                                await Clients.Client(Connection_Handle.userConnectionids[userID]).SendAsync("userLeft_Channel", true, Kicked_userData.LoginID, userData.ChannelID);
                            }

                            Dictionary<string, string> msg = new Dictionary<string, string>
                            {
                                {"message", Kicked_userData.Name + " got kicked from server by an Admin." }
                            };

                            await Clients.Group(userData.ChannelID).SendAsync("ReceiveMessage", "Server", msg, "-1", "-1", ApiVariable.IPConnection + "Users/default.png");
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Ban_User(string userID, string reason, string duration)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key='{0}'", userData.ChannelID);
                        string admin_id = cmd.ExecuteScalar().ToString();

                        if (userData.LoginID == admin_id)
                        {
                            string ban_expire = duration == "0" ? "Never" : DateTime.Now.AddMinutes(double.Parse(duration)).ToString("dd/MM/yyyy H:mm");

                            cmd.CommandText = String.Format("INSERT INTO bans (channel_id, banned_id, admin_id, reason, duration, ban_date, ban_expire) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}')", userData.ChannelID, userID, userData.LoginID, reason, duration, DateTime.Now.ToString("dd/MM/yyyy H:mm"), ban_expire);
                            cmd.ExecuteScalar();

                            cmd.CommandText = String.Format("DELETE FROM users_in_channels WHERE user_id = '{0}' AND channel_id = '{1}'", userID, userData.ChannelID);
                            cmd.ExecuteScalar();

                            cmd.CommandText = String.Format("SELECT name FROM users WHERE loginid = '{0}'", userID);
                            string banned_name = cmd.ExecuteScalar().ToString();

                            _connectionInfo.TryGetValue(Connection_Handle.userConnectionids[userID], out User Kicked_userData);

                            await Clients.Group(userData.ChannelID).SendAsync("userLeft_Channel", true, Kicked_userData.LoginID, userData.ChannelID);

                            if (Kicked_userData.ChannelID == userData.ChannelID)
                            {
                                await Clients.Group(userData.ChannelID).SendAsync("WritingMessage_Channel", Kicked_userData.Name, userData.ChannelID, false, Kicked_userData.LoginID);
                                await Groups.RemoveFromGroupAsync(Connection_Handle.userConnectionids[userID], Kicked_userData.ChannelID);
                                Kicked_userData.ChannelID = null;
                            }
                            else
                            {
                                await Clients.Client(Connection_Handle.userConnectionids[userID]).SendAsync("userLeft_Channel", true, Kicked_userData.LoginID, userData.ChannelID);
                            }

                            Dictionary<string, string> msg = new Dictionary<string, string>
                            {
                                {"message", $"{banned_name} got banned from server by an Admin for: {reason}" }
                            };

                            await Clients.Group(userData.ChannelID).SendAsync("ReceiveMessage", "Server", msg, "-1", "-1", ApiVariable.IPConnection + "Users/default.png");
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Transfer_channelAdmin(string userID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key='{0}'", userData.ChannelID);
                        string admin_id = cmd.ExecuteScalar().ToString();

                        if (userData.LoginID == admin_id)
                        {
                            cmd.CommandText = String.Format("UPDATE channels SET channel_admin = '{0}' WHERE channel_key = '{1}'", userID, userData.ChannelID);
                            cmd.ExecuteScalar();
                            await Clients.Group(userData.ChannelID).SendAsync("Transfered_channelAdmin", true, userID, userData.LoginID);
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Get_Banlist()
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    List<Online_Users_inChannel> foundUsers = new List<Online_Users_inChannel>();
                    List<string> avatar_links = new List<string>();
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT banned_id, name, status, avatar FROM users,bans WHERE channel_id = '{0}' AND loginid = banned_id", userData.ChannelID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                Online_Users_inChannel user = new Online_Users_inChannel();
                                user.Name = readed.GetString("name");
                                user.userID = readed.GetString("banned_id");
                                if (readed.GetString("status") == "offline")
                                {
                                    user.Status = "Gray";
                                }
                                else
                                {
                                    user.Status = readed.GetString("status") == "online" ? "Green" : "Yellow";
                                }
                                if (readed.GetString("avatar") != "default")
                                {
                                    avatar_links.Add(ApiVariable.IPConnection + "Users/" + readed.GetString("banned_id") + "/" + readed.GetString("avatar") + ".png");
                                }
                                else
                                {
                                    avatar_links.Add(ApiVariable.IPConnection + "Users/default.png");
                                }
                                foundUsers.Add(user);
                            }
                            readed.Close();
                        }
                        await Clients.Client(Context.ConnectionId).SendAsync("Got_Banlist", foundUsers, foundUsers.Count, avatar_links);
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Unban_user(string userID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key='{0}'", userData.ChannelID);
                        string admin_id = cmd.ExecuteScalar().ToString();

                        if (userData.LoginID == admin_id)
                        {
                            cmd.CommandText = String.Format("DELETE FROM bans WHERE banned_id = '{0}' AND channel_id = '{1}'", userID, userData.ChannelID);
                            cmd.ExecuteScalar();
                            await Clients.Client(Context.ConnectionId).SendAsync("Unbanned_user", true, userID);
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Send_friendRequest(string userID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT COUNT(*) FROM friends WHERE (user1_id = '{0}' AND user2_id = '{1}') OR (user1_id = '{1}' AND user2_id = '{0}')", userID, userData.LoginID);
                        int found = Int32.Parse(cmd.ExecuteScalar().ToString());

                        if (found > 0)
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("Sent_friendRequest", false);
                        }
                        else
                        {
                            cmd.CommandText = String.Format("INSERT INTO friends (user1_id, user2_id, status) VALUES ('{0}', '{1}', '{2}')", userID, userData.LoginID, "Pending");
                            cmd.ExecuteScalar();

                            Online_Users_inChannel user = new Online_Users_inChannel();
                            user.userID = userData.LoginID;
                            user.Name = userData.Name;
                            string url;

                            if (userData.AvatarID != "default")
                            {
                                url = ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + userData.AvatarID + ".png";
                            }
                            else
                            {
                                url = ApiVariable.IPConnection + "Users/default.png";
                            }

                            await Clients.Client(Connection_Handle.userConnectionids[userID]).SendAsync("newPending_friendRequest", user, url);
                            await Clients.Client(Context.ConnectionId).SendAsync("Sent_friendRequest", true);
                        }
                    }
                }
                catch (Exception) { await Clients.Client(Context.ConnectionId).SendAsync("Sent_friendRequest", false); }
            }
        }
        public async Task Accept_friendRequest(string userID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        Online_Users_inChannel own_data = new Online_Users_inChannel();
                        Online_Users_inChannel friend_data = new Online_Users_inChannel();
                        string friend_avatar = string.Empty;

                        cmd.CommandText = String.Format("UPDATE friends SET status = '{0}' WHERE (user1_id = '{1}' AND user2_id = '{2}') OR (user1_id = '{2}' AND user2_id = '{1}')", "Friends", userID, userData.LoginID);
                        cmd.ExecuteScalar();

                        cmd.CommandText = String.Format("SELECT name, status, avatar FROM users WHERE loginid = '{0}'", userID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                friend_data.Name = readed.GetString("name");
                                friend_data.userID = userID;
                                if (readed.GetString("status") == "offline")
                                {
                                    friend_data.Status = "Gray";
                                }
                                else
                                {
                                    friend_data.Status = readed.GetString("status") == "online" ? "Green" : "Yellow";
                                }
                                friend_avatar = readed.GetString("avatar");
                            }
                            readed.Close();
                        }

                        cmd.CommandText = String.Format("SELECT name, status FROM users WHERE loginid = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                own_data.Name = readed.GetString("name");
                                own_data.userID = userData.LoginID;
                                if (readed.GetString("status") == "offline")
                                {
                                    own_data.Status = "Gray";
                                }
                                else
                                {
                                    own_data.Status = readed.GetString("status") == "online" ? "Green" : "Yellow";
                                }
                            }
                            readed.Close();
                        }

                        string url;

                        if (userData.AvatarID != "default")
                        {
                            url = ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + userData.AvatarID + ".png";
                        }
                        else
                        {
                            url = ApiVariable.IPConnection + "Users/default.png";
                        }

                        await Clients.Client(Connection_Handle.userConnectionids[userID]).SendAsync("new_friendAdded", own_data, url);
                        await Clients.Client(Context.ConnectionId).SendAsync("Accepted_friendRequest", userID);

                        if (friend_avatar != "default")
                        {
                            url = ApiVariable.IPConnection + "Users/" + userID + "/" + friend_avatar + ".png";
                        }
                        else
                        {
                            url = ApiVariable.IPConnection + "Users/default.png";
                        }

                        await Clients.Client(Context.ConnectionId).SendAsync("new_friendAdded", friend_data, url);
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Reject_Pending(string userID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("DELETE FROM friends WHERE (user1_id = '{0}' AND user2_id = '{1}') OR (user1_id = '{1}' AND user2_id = '{0}')", userID, userData.LoginID);
                        cmd.ExecuteScalar();
                        await Clients.Client(Context.ConnectionId).SendAsync("Rejected_Pending", userID);
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Delete_Friend(string userID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("DELETE FROM friends WHERE (user1_id = '{0}' AND user2_id = '{1}') OR (user1_id = '{1}' AND user2_id = '{0}')", userID, userData.LoginID);
                        cmd.ExecuteScalar();

                        await Clients.Client(Connection_Handle.userConnectionids[userID]).SendAsync("Deleted_asFriend", userData.LoginID);
                        await Clients.Client(Context.ConnectionId).SendAsync("Deleted_asFriend", userID);
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Check_privateChannel(string userID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        Private_Users user = new Private_Users();
                        string[] urls = new string[2];

                        cmd.CommandText = String.Format("SELECT COUNT(*) FROM private_channels WHERE (user1_id = '{0}' AND user2_id = '{1}') OR (user1_id = '{1}' AND user2_id = '{0}')", userID, userData.LoginID);
                        int found = Int32.Parse(cmd.ExecuteScalar().ToString());
                        if (found == 0)
                        {
                            Guid id = Guid.NewGuid();
                            cmd.CommandText = String.Format("INSERT INTO private_channels (channelID, user1_id, user2_id) VALUES ('{0}', '{1}' ,'{2}')", id.ToString(), userID, userData.LoginID);
                            cmd.ExecuteScalar();

                            Directory.CreateDirectory("C:/xampp/htdocs/Channels/" + id);

                            cmd.CommandText = String.Format("SELECT name, status, avatar FROM users WHERE loginid = '{0}'", userID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    user.Name = readed.GetString("name");
                                    user.userID = userID;
                                    user.channelID = id.ToString();
                                    if (readed.GetString("status") == "offline")
                                    {
                                        user.Status = "Gray";
                                    }
                                    else
                                    {
                                        user.Status = readed.GetString("status") == "online" ? "Green" : "Yellow";
                                    }

                                    if (readed.GetString("avatar") != "default")
                                    {
                                        urls[0] = ApiVariable.IPConnection + "Users/" + userID + "/" + readed.GetString("avatar") + ".png";
                                    }
                                    else
                                    {
                                        urls[0] = ApiVariable.IPConnection + "Users/default.png";
                                    }
                                }
                                readed.Close();
                            }

                            if (Connection_Handle.userConnectionids.TryGetValue(userID, out string ConnectionID))
                            {
                                Private_Users user1 = new Private_Users();
                                user1.Name = userData.Name;
                                user1.userID = userData.LoginID;
                                user1.channelID = id.ToString();
                                cmd.CommandText = String.Format("SELECT status FROM users WHERE loginid = '{0}'", userData.LoginID);
                                string status = cmd.ExecuteScalar().ToString();
                                if (status == "offline")
                                {
                                    user1.Status = "Gray";
                                }
                                else
                                {
                                    user1.Status = status == "online" ? "Green" : "Yellow";
                                }

                                if (userData.AvatarID != "default")
                                {
                                    urls[1] = ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + userData.AvatarID + ".png";
                                }
                                else
                                {
                                    urls[1] = ApiVariable.IPConnection + "Users/default.png";
                                }

                                await Clients.Client(ConnectionID).SendAsync("Checked_privateChannel", user1, found, urls[1], userData.LoginID);
                            }
                        }
                        else
                        {
                            cmd.CommandText = String.Format("SELECT channelID FROM private_channels WHERE (user1_id = '{0}' AND user2_id = '{1}') OR (user1_id = '{1}' AND user2_id = '{0}')", userID, userData.LoginID);
                            user.channelID = cmd.ExecuteScalar().ToString();

                            cmd.CommandText = String.Format("SELECT Blocker_ID FROM blocked WHERE Channel_ID = '{0}'", user.channelID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    user.isBlocked = readed.GetString("Blocker_ID") == userData.LoginID ? "Unblock User" : "Block User";
                                }
                                readed.Close();
                            }
                        }

                        await Clients.Client(Context.ConnectionId).SendAsync("Checked_privateChannel", user, found, urls[0], userData.LoginID);

                    }
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }
        public async Task Block_privateUser(string channelID, string userID, string isBlocked)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        if (isBlocked == "Block User")
                        {
                            cmd.CommandText = String.Format("INSERT INTO blocked (Blocker_ID, Blocked_ID, Channel_ID) VALUES ('{0}', '{1}' ,'{2}')", userData.LoginID, userID, channelID);
                            cmd.ExecuteScalar();
                            isBlocked = "Unblock User";
                        }
                        else
                        {
                            cmd.CommandText = String.Format("DELETE FROM blocked WHERE Channel_ID = '{0}' AND Blocker_ID = '{1}'", channelID, userData.LoginID);
                            cmd.ExecuteScalar();
                            isBlocked = "Block User";
                        }

                        await Clients.Client(Context.ConnectionId).SendAsync("Blocked_privateUser", channelID, isBlocked);

                    }
                }
                catch (Exception) { }
            }
        }
        public async Task User_changeAvatar(string ImageID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    List<string> channel_ids = new List<string>();
                    List<string> Friends_ids = new List<string>();
                    List<string> Privates_ids = new List<string>();

                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("UPDATE users SET avatar = '{0}' WHERE loginid = '{1}'", ImageID, userData.LoginID);
                        cmd.ExecuteScalar();

                        userData.AvatarID = ImageID;

                        cmd.CommandText = String.Format("SELECT channel_id FROM users_in_channels WHERE user_id = '{0}';", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                channel_ids.Add(readed.GetString("channel_id"));
                            }
                            readed.Close();
                        }

                        cmd.CommandText = String.Format("SELECT channelID FROM private_channels WHERE user1_id = '{0}' OR user2_id = '{0}';", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                channel_ids.Add(readed.GetString("channelID"));
                            }
                            readed.Close();
                        }

                        cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM friends WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                if (readed.GetString("user1_id") != userData.LoginID)
                                {
                                    Friends_ids.Add(readed.GetString("user1_id"));
                                }
                                else if (readed.GetString("user2_id") != userData.LoginID)
                                {
                                    Friends_ids.Add(readed.GetString("user2_id"));
                                }
                            }
                            readed.Close();
                        }

                        cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM private_channels WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                if (readed.GetString("user1_id") != userData.LoginID)
                                {
                                    Privates_ids.Add(readed.GetString("user1_id"));
                                }
                                else if (readed.GetString("user2_id") != userData.LoginID)
                                {
                                    Privates_ids.Add(readed.GetString("user2_id"));
                                }
                            }
                            readed.Close();
                        }

                        foreach (string id in Privates_ids)
                        {
                            if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                            {
                                await Clients.Client(ConnectionID).SendAsync("Private_changedAvatar", userData.LoginID, ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + ImageID + ".png");
                            }
                        }

                        foreach (string id in Friends_ids)
                        {
                            if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                            {
                                await Clients.Client(ConnectionID).SendAsync("Friend_changedAvatar", userData.LoginID, ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + ImageID + ".png");
                            }
                        }

                        foreach (string id in channel_ids)
                        {
                            await Clients.Group(id).SendAsync("User_changedAvatar", userData.LoginID, ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + ImageID + ".png");
                        }

                        if (userData.ChannelID == null || userData.ChannelID == String.Empty)
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("User_changedAvatar", userData.LoginID, ApiVariable.IPConnection + "Users/" + userData.LoginID + "/" + ImageID + ".png");
                        }
                    }
                }
                catch (Exception) { }
            }
        }
        public async Task Channel_changeAvatar(string ImageID)
        {
            if (_connectionInfo.TryGetValue(Context.ConnectionId, out User userData))
            {
                try
                {
                    using (MySqlCommand cmd = Startup.connection.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key = '{0}'", userData.ChannelID);
                        string adminid = cmd.ExecuteScalar().ToString();

                        if (adminid == userData.LoginID)
                        {
                            List<string> users_in_channel = new List<string>();

                            cmd.CommandText = String.Format("UPDATE channels SET image = '{0}' WHERE channel_key = '{1}'", ImageID, userData.ChannelID);
                            cmd.ExecuteScalar();

                            cmd.CommandText = String.Format("SELECT user_id FROM users_in_channels WHERE channel_id = '{0}'", userData.ChannelID);
                            using (MySqlDataReader readed = cmd.ExecuteReader())
                            {
                                while (readed.Read())
                                {
                                    users_in_channel.Add(readed.GetString("user_id"));
                                }
                                readed.Close();
                            }

                            foreach (string user in users_in_channel)
                            {
                                if (Connection_Handle.userConnectionids.TryGetValue(user, out string ConnectionID))
                                {
                                    await Clients.Client(ConnectionID).SendAsync("Channel_changedAvatar", userData.ChannelID, ApiVariable.IPConnection + "Channels/" + ImageID + ".png");
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        //Normal functions
        private async void LoginToAccount(string loginid, string status, string channelID)
        {
            try
            {
                using (MySqlCommand cmd = Startup.connection.CreateCommand())
                {
                    List<string> channel_ids = new List<string>();
                    List<string> Friends_ids = new List<string>();
                    List<string> Privates_ids = new List<string>();
                    string name = String.Empty;
                    string avatar = String.Empty;

                    cmd.CommandText = String.Format("SELECT name, avatar FROM users WHERE loginid = '{0}'", loginid);
                    using (MySqlDataReader readed = cmd.ExecuteReader())
                    {
                        while (readed.Read())
                        {
                            name = readed.GetString("name");
                            avatar = readed.GetString("avatar");
                        }
                        readed.Close();
                    }

                    if (status != "online" && status != "away" && status != "offline")
                    {
                        status = "online";
                    }

                    cmd.CommandText = String.Format("UPDATE users SET status = '{0}' WHERE loginid = '{1}'", status, loginid);
                    cmd.ExecuteScalar();

                    User userData = new User();
                    userData.Name = name;
                    userData.ChannelID = channelID;
                    userData.LoginID = loginid;
                    userData.AvatarID = avatar;
                    _connectionInfo[Context.ConnectionId] = userData;
                    Connection_Handle.userConnectionids.Add(loginid, Context.ConnectionId);

                    Online_Users_inChannel user = new Online_Users_inChannel();
                    user.Name = name;
                    user.userID = loginid;
                    user.Status = status == "online" ? "Green" : "Yellow";

                    if (status != "offline")
                    {
                        cmd.CommandText = String.Format("SELECT channel_id FROM users_in_channels WHERE user_id = '{0}';", loginid);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                channel_ids.Add(readed.GetString("channel_id"));
                            }
                            readed.Close();
                        }

                        cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM friends WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                if (readed.GetString("user1_id") != userData.LoginID)
                                {
                                    Friends_ids.Add(readed.GetString("user1_id"));
                                }
                                else if (readed.GetString("user2_id") != userData.LoginID)
                                {
                                    Friends_ids.Add(readed.GetString("user2_id"));
                                }
                            }
                            readed.Close();
                        }

                        cmd.CommandText = String.Format("SELECT user1_id, user2_id FROM private_channels WHERE user1_id = '{0}' OR user2_id = '{0}'", userData.LoginID);
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                if (readed.GetString("user1_id") != userData.LoginID)
                                {
                                    Privates_ids.Add(readed.GetString("user1_id"));
                                }
                                else if (readed.GetString("user2_id") != userData.LoginID)
                                {
                                    Privates_ids.Add(readed.GetString("user2_id"));
                                }
                            }
                            readed.Close();
                        }

                        foreach (string id in Privates_ids)
                        {
                            if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                            {
                                await Clients.Client(ConnectionID).SendAsync("Private_changeStatus", user);
                            }
                        }

                        foreach (string id in Friends_ids)
                        {
                            if (Connection_Handle.userConnectionids.TryGetValue(id, out string ConnectionID))
                            {
                                await Clients.Client(ConnectionID).SendAsync("Friend_Online", user);
                            }
                        }

                        foreach (string id in channel_ids)
                        {
                            cmd.CommandText = String.Format("SELECT channel_admin FROM channels WHERE channel_key = '{0}'", id);
                            string admin = cmd.ExecuteScalar().ToString();
                            user.isAdmin = admin == loginid ? "Visible" : "Collapsed";
                            await Clients.Group(id).SendAsync("User_Online", user, ApiVariable.IPConnection + "Users/" + id + "/" + avatar + ".png");
                        }
                    }
                    Console.WriteLine(userData.Name + " logged in with ID: " + userData.LoginID + " / " + Context.ConnectionId);
                }
            }
            catch (Exception) { }
        }
    }
}
