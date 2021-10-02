using Halmid_Client.Chat_Functions;
using Halmid_Client.Connectors;
using Halmid_Client.Functions;
using Halmid_Client.Variables;
using Halmid_Client.Windows.Banned_Message;
using Halmid_Client.Windows.Channel_Settings;
using Halmid_Client.Windows.Confirmation;
using Halmid_Client.Windows.Emotes;
using Halmid_Client.Windows.ImagePreview;
using Halmid_Client.Windows.ImageResolution;
using Halmid_Client.Windows.Loading;
using Halmid_Client.Windows.Login;
using Halmid_Client.Windows.NewChannel;
using Halmid_Client.Windows.Search_User;
using Halmid_Client.Windows.User_Settings;
using Halmid_Client.Windows.userBan_Settings;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;

namespace Halmid_Client
{

    public partial class MainWindow : Window
    {
        ObservableCollection<MessageDataView> MessagesData = new ObservableCollection<MessageDataView>();
        ObservableCollection<Online_Users_inChannel> OnlineUsers_Data = new ObservableCollection<Online_Users_inChannel>();
        List<MessageSender_Handler> MessageSender_Data = new List<MessageSender_Handler>();
        System.Windows.Forms.NotifyIcon m_notifyIcon;
        AddChannel newChannel;
        StackPanel panel;
        int previousLineCount = 0;
        bool Writing = false;
        bool logged_out = false;
        static int channelindex;
        string oldmessage;

        public MainWindow()
        {
            InitializeComponent();
            Create_NotifyIcon();
            Window_Resize();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterEvents();
            UpdateUsername();
            RegisterData();
            if (UserData.ChannelID != null)
            {
                if (Global_Variables.channelType == "channel")
                {
                    await Connector.connection.SendAsync("Switch_Channel", UserData.ChannelID, "channel");
                }
                else
                {
                    await Connector.connection.SendAsync("Switch_Channel", UserData.ChannelID, "private");
                }
            }
        }
        private void Window_OnClosing(object sender, EventArgs e)
        {
            Save_Windowsize.Save(this.ActualWidth, this.ActualHeight);
        }
        private void Window_Resize()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
            var height = doc.SelectSingleNode("//Config/WindowData/Window_Height").InnerText;
            var width = doc.SelectSingleNode("//Config/WindowData/Window_Width").InnerText;
            if (height != String.Empty && width != String.Empty)
            {
                this.Width = double.Parse(width);
                this.Height = double.Parse(height);
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.V && Clipboard.ContainsImage() && UserData.ChannelID != null && UserData.ChannelID != String.Empty && Launch_Cover.Visibility == Visibility.Hidden)
                {

                    Preview_Window preview = new Preview_Window("", "");
                    preview.Owner = this;
                    preview.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    Channel_Grid.Opacity = 0.4;
                    Priv_Grid.Opacity = 0.4;
                    msg_grid.Opacity = 0.4;
                    Online_Grid.Opacity = 0.4;

                    preview.ShowDialog();

                    Channel_Grid.Opacity = 1;
                    Priv_Grid.Opacity = 1;
                    msg_grid.Opacity = 1;
                    Online_Grid.Opacity = 1;
                    this.Topmost = true;
                    Task.Delay(50);
                    this.Topmost = false;
                }
            }
        }
        private void Window_MouseDown(object sender, MouseEventArgs e)
        {
            if (Status_Frame.Visibility == Visibility.Visible)
            {
                Status_Frame.Visibility = Visibility.Collapsed;
            }
        }
        private void RegisterData()
        {
            ListViewChannels.ItemsSource = _ChannelList.Channels;
            ListViewMessages.ItemsSource = MessagesData;
            ListViewPrivates.ItemsSource = _Private_Users.PrivateUsers_Data;

            if (UserData.Status == "Gray")
            {
                Profile_Status.Background = Brushes.Gray;
            }
            else{
                Profile_Status.Background = UserData.Status == "Green" ? Brushes.Green : Brushes.Yellow;
            }
            Profile_Image.ImageSource = UserData.Avatar;
            Launch_Cover.Content = new Search_User_Window();
            Online_Grid.Visibility = Visibility.Collapsed;
            Launch_Cover.Visibility = Visibility.Visible;
            msg_grid.SetValue(Grid.ColumnSpanProperty, 2);
        }
        private void Create_NotifyIcon()
        {
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.Text = "Halmid";
            m_notifyIcon.Icon = new System.Drawing.Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/logo.ico")).Stream);
            m_notifyIcon.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    m_notifyIcon.Visible = false;
                    this.WindowState = WindowState.Normal;
                };
        }
        private void UpdateUsername()
        {
            if (UserData.Name.Length > 13)
            {
                UserName.Text = UserData.Name.Substring(0, 13) + "...";
            }
            else
            {
                UserName.Text = UserData.Name;
            }
        }
        private void RegisterEvents()
        {
            try
            {
                Connector.connection.Closed += (error) =>
                {
                    if (logged_out == false)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => { 
                            Loading_Window Loading = new Loading_Window("Reconnect");
                            Loading.Show();
                            Close();
                        }));
                    }
                    return null;
                };
                Connector.connection.On<bool>("clientUpdate_Data", isUpdate =>
                {
                    if(isUpdate)
                    {
                        Update_Button.Visibility = Visibility.Visible;
                    }
                });
                Connector.connection.On<string, string, string, string, string>("ReceiveMessage", (name, message, messid, userID, avatar) =>
                {
                    Application.Current.Dispatcher.Invoke((Action)async delegate
                    {
                        MessageDataView data = new MessageDataView();

                        if (MessageSender_Data.Find(x => x.userID == userID) != null)
                        {
                            data.Avatar = MessageSender_Data.FirstOrDefault(x => x.userID == userID).Avatar;
                        }
                        else
                        {
                            MessageSender_Handler new_user = new MessageSender_Handler();
                            new_user.userID = data.Sender_id;
                            new_user.Avatar = await ToBitmapImage.Coverter(480, avatar);
                            data.Avatar = new_user.Avatar;
                            MessageSender_Data.Add(new_user);
                        }
                        data.MessageID = messid;
                        data.Sender_id = userID;
                        data.Content = message;
                        data.From = name;
                        data.Timestamp = DateTime.Now.ToString("dd/MM/yyyy - H:mm");
                        data.Colored = UserData.LoginID == userID ? "DarkGreen" : "White";
                        MessagesData.Add(data);
                        ListViewMessages.Items.MoveCurrentToLast();
                        ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);
                    });
                });
                Connector.connection.On<string, string, string, string, string, string>("Receive_ImageMessage", async (name, message, imageID, messid, userID, avatar) =>
                {
                    MessageDataView data = new MessageDataView();
                    if (MessageSender_Data.Find(x => x.userID == userID) != null)
                    {
                        data.Avatar = MessageSender_Data.FirstOrDefault(x => x.userID == userID).Avatar;
                    }
                    else
                    {
                        MessageSender_Handler new_user = new MessageSender_Handler();
                        new_user.userID = data.Sender_id;
                        new_user.Avatar = await ToBitmapImage.Coverter(480, avatar);
                        data.Avatar = new_user.Avatar;
                        MessageSender_Data.Add(new_user);
                    }
                    data.ImageSource = await ToBitmapImage.Coverter(1280, imageID);
                    data.MessageID = messid;
                    data.Sender_id = userID;
                    data.Content = message;
                    data.From = name;
                    data.Timestamp = DateTime.Now.ToString("dd/MM/yyyy - H:mm");
                    data.Colored = UserData.LoginID == userID ? "DarkGreen" : "White";
                    data.ImageID = imageID;
                    MessagesData.Add(data);
                    ListViewMessages.Items.MoveCurrentToLast();
                    ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);
                });
                Connector.connection.On<string, string, bool, string>("WritingMessage_Channel", (name, roomid, isWriting, userID) =>
                {
                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (UserData.LoginID != userID)
                        {
                            WritingUsers_Text.Text = WritingMessage_Notification.WritingMessage_Channel(name, roomid, isWriting, userID);
                        }
                    });
                });
                Connector.connection.On<bool, string, string, string>("Created_Channel", async (created, id, name, url) =>
                {
                    if (created)
                    {
                        newChannel.Close();
                        ChannelList channel_data = new ChannelList();
                        channel_data.Name = name;
                        channel_data.ChannelHash = id;
                        channel_data.Avatar = await ToBitmapImage.Coverter(480, url);
                        _ChannelList.Channels.Add(channel_data);
                        channelindex = ListViewChannels.Items.Count - 1;

                        ListViewChannels.Items.MoveCurrentToLast();
                        ListViewChannels.ScrollIntoView(ListViewChannels.Items[ListViewChannels.Items.Count - 1]);

                        await Connector.connection.SendAsync("Switch_Channel", _ChannelList.Channels[ListViewChannels.Items.Count - 1].ChannelHash);
                    }
                });
                Connector.connection.On<Online_Users_inChannel>("User_Offline", user =>
                {
                    OnlineUsers_Data.Where(x => x.userID == user.userID).Single().Status = "Gray";
                    ListViewUsers.ItemsSource = OnlineUsers_Data.Where(x => x.Status != "Gray");
                });
                Connector.connection.On<Online_Users_inChannel, string>("User_Online", async (user, url) =>
                {
                    if (OnlineUsers_Data.FirstOrDefault(x => x.userID == user.userID) != null)
                    {
                        OnlineUsers_Data.FirstOrDefault(x => x.userID == user.userID).Status = user.Status;
                    }
                    else
                    {
                        user.Avatar = await ToBitmapImage.Coverter(480, url);
                        OnlineUsers_Data.Add(user);
                    }
                    ListViewUsers.ItemsSource = OnlineUsers_Data.Where(x => x.Status != "Gray");
                });
                Connector.connection.On<bool, ObservableCollection<MessageDataView>, ObservableCollection<Online_Users_inChannel>, string, List<string>, List<string>>("Switched_Channel", async(switched, messages, users, isAdmin, msg_url, online_url) =>
                {
                    try
                    {
                        if (switched)
                        {
                            int i = 0;
                            int j = 0;
                            MessageSender_Data.Clear();
                            await Task.Run(async () => {
                                await this.Dispatcher.BeginInvoke(new ThreadStart(async() =>
                                {
                                    foreach (MessageDataView data in messages)
                                    {
                                        if(MessageSender_Data.Find(x => x.userID == data.Sender_id) != null)
                                        {
                                            data.Avatar = MessageSender_Data.FirstOrDefault(x => x.userID == data.Sender_id).Avatar;
                                        }
                                        else
                                        {
                                            MessageSender_Handler new_user = new MessageSender_Handler();
                                            new_user.userID = data.Sender_id;
                                            new_user.Avatar = await ToBitmapImage.Coverter(480, msg_url[i]);
                                            data.Avatar = new_user.Avatar;
                                            MessageSender_Data.Add(new_user);
                                        }

                                        if (data.ImageID != null && data.ImageID != String.Empty)
                                        {
                                            data.ImageSource = await ToBitmapImage.Coverter(480, data.ImageID);
                                        }
                                        i++;
                                    }

                                    foreach (Online_Users_inChannel data in users)
                                    {
                                        data.Avatar = await ToBitmapImage.Coverter(960, online_url[j]);
                                        j++;
                                    }
                                }));
                            });

                            MessagesData.Clear();
                            OnlineUsers_Data.Clear();
                            Channel_Name.Text = "#" + _ChannelList.Channels[channelindex].Name;
                            UserData.ChannelID = _ChannelList.Channels[channelindex].ChannelHash;
                            MessagesData = messages;
                            OnlineUsers_Data = users;
                            ListViewUsers.ItemsSource = OnlineUsers_Data;
                            ListViewMessages.ItemsSource = MessagesData;
                            ListViewMessages.Items.MoveCurrentToLast();
                            ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);
                            msg.Opacity = 0.3;
                            msg.Text = "Message " + Channel_Name.Text;
                            msg.IsEnabled = true;
                            WritingUsers_Text.Text = "";
                            Loading_Panel.Visibility = Visibility.Collapsed;
                            Global_Variables.channelType = "channel";
                            channelSettings_Button.Visibility = Visibility.Visible;

                            if (Online_Grid.Visibility == Visibility.Collapsed)
                            {
                                Online_Grid.Visibility = Visibility.Visible;
                                Launch_Cover.Visibility = Visibility.Hidden;
                                msg_grid.SetValue(Grid.ColumnSpanProperty, 1);
                            }

                            if (isAdmin == UserData.LoginID)
                            {
                                leaveServer_Button.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                leaveServer_Button.Visibility = Visibility.Visible;
                            }

                            ListViewMessages.Visibility = Visibility.Visible;
                            ListViewMessages.Items.Refresh();
                        }
                    }
                    catch (Exception e) { MessageBox.Show(e.ToString()); }
                });
                Connector.connection.On<bool, string, string, ObservableCollection<MessageDataView>, ObservableCollection<Online_Users_inChannel>, List<string>, List<string>, string>("Joined_Channel", async (joined, channelname, channelid, messages, users, msg_url, online_url, avatar_url) =>
                {
                    try
                    {
                        if (joined)
                        {
                            int i = 0;
                            int j = 0;
                            await Task.Run(async () =>
                            {
                                await this.Dispatcher.BeginInvoke(new ThreadStart(async () =>
                                {
                                    MessageSender_Data.Clear();
                                    foreach (MessageDataView data in messages)
                                    {
                                        if (MessageSender_Data.Find(x => x.userID == data.Sender_id) != null)
                                        {
                                            data.Avatar = MessageSender_Data.FirstOrDefault(x => x.userID == data.Sender_id).Avatar;
                                        }
                                        else
                                        {
                                            MessageSender_Handler new_user = new MessageSender_Handler();
                                            new_user.userID = data.Sender_id;
                                            new_user.Avatar = await ToBitmapImage.Coverter(480, msg_url[i]);
                                            data.Avatar = new_user.Avatar;
                                            MessageSender_Data.Add(new_user);
                                        }
                                        if (data.ImageID != null && data.ImageID != String.Empty)
                                        {
                                            data.ImageSource = await ToBitmapImage.Coverter(480, data.ImageID);
                                        }
                                        i++;
                                    }

                                    foreach (Online_Users_inChannel data in users)
                                    {
                                        data.Avatar = await ToBitmapImage.Coverter(960, online_url[j]);
                                        j++;
                                    }
                                }));
                            });

                            UserData.ChannelID = channelid;
                            newChannel.Close();

                            ChannelList channel = new ChannelList();
                            channel.Avatar = await ToBitmapImage.Coverter(480, avatar_url);
                            channel.Name = channelname;
                            channel.ChannelHash = channelid;
                            Channel_Name.Text = "# " + channelname;
                            _ChannelList.Channels.Add(channel);

                            MessagesData.Clear();
                            OnlineUsers_Data.Clear();
                            OnlineUsers_Data = users;
                            MessagesData = messages;
                            ListViewMessages.ItemsSource = MessagesData;
                            ListViewUsers.ItemsSource = OnlineUsers_Data;
                            ListViewMessages.Items.MoveCurrentToLast();
                            ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);
                            msg.Opacity = 0.3;
                            msg.Text = "Message " + Channel_Name.Text;
                            msg.IsEnabled = true;
                            WritingUsers_Text.Text = "";

                            Loading_Panel.Visibility = Visibility.Collapsed;
                            Global_Variables.channelType = "channel";
                            channelSettings_Button.Visibility = Visibility.Visible;

                            if (Online_Grid.Visibility == Visibility.Collapsed)
                            {
                                Online_Grid.Visibility = Visibility.Visible;
                                Launch_Cover.Visibility = Visibility.Hidden;
                                msg_grid.SetValue(Grid.ColumnSpanProperty, 1);
                            }

                            leaveServer_Button.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            newChannel.server_id.IsEnabled = true;
                            newChannel.Join_Button.IsEnabled = true;
                            newChannel.wrong_text.Visibility = Visibility.Visible;
                        }
                    }
                    catch(Exception e) { MessageBox.Show(e.ToString()); }
                });
                Connector.connection.On<bool>("Check_channelAdmin_user", isAdmin =>
                {
                    Channel_Settings_Window settings = new Channel_Settings_Window();
                    settings.Owner = this;
                    settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    Channel_Grid.Opacity = 0.4;
                    Priv_Grid.Opacity = 0.4;
                    msg_grid.Opacity = 0.4;
                    Online_Grid.Opacity = 0.4;

                    if (isAdmin)
                    {
                        settings.Height = 470;
                    }
                    else
                    {
                        settings.Height = 260;
                    }

                    settings.Show();
                    settings.Closed += (s, c) =>
                    {
                        Channel_Grid.Opacity = 1;
                        Priv_Grid.Opacity = 1;
                        msg_grid.Opacity = 1;
                        Online_Grid.Opacity = 1;
                        this.Topmost = true;
                        Task.Delay(50);
                        this.Topmost = false;
                    };
                });
                Connector.connection.On<bool, string>("Update_Name", (updated, new_name) =>
                {
                    if (updated)
                    {
                        UserData.Name = new_name;
                        UserName.Text = new_name;
                    }
                });
                Connector.connection.On<string, string>("User_Updated_Name", (name, userid) =>
                {
                    var found = OnlineUsers_Data.FirstOrDefault(x => x.userID == userid);
                    if (found != null)
                    {
                        found.Name = name;
                    }

                    foreach (MessageDataView data in MessagesData)
                    {
                        if (data.Sender_id == userid)
                        {
                            data.From = name;
                        }
                    }
                    ListViewMessages.Items.Refresh();
                    ListViewUsers.Items.Refresh();
                });
                Connector.connection.On<bool, string>("Updated_Login", (isUpdated, username) =>
                    {
                        if (isUpdated)
                        {
                            UserData.Username = username;
                        }
                    });
                Connector.connection.On<bool, string, string>("Updated_Channel_Name", (isUpdated, name, id) =>
                {
                    if (isUpdated)
                    {
                        MessageBox.Show("ok");
                        var found = _ChannelList.Channels.FirstOrDefault(x => x.ChannelHash == id);
                        if (found != null)
                        {
                            found.Name = name;
                        }

                        if (UserData.ChannelID == id)
                        {
                            Channel_Name.Text = "# " + name;
                        }
                    }
                });
                Connector.connection.On<Online_Users_inChannel, bool, string>("Changed_userStatus", async (user, isChanged, url) =>
                {
                    if (isChanged)
                    {
                        if (user.userID == UserData.LoginID)
                        {
                            UserData.Status = user.Status;
                            XDocument doc = XDocument.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
                            var data = doc.Root.Descendants("Status").FirstOrDefault();
                            switch (user.Status)
                            {
                                case "Green":
                                    Profile_Status.Background = Brushes.Green;
                                    data.SetValue("online");
                                    break;
                                case "Yellow":
                                    Profile_Status.Background = Brushes.Yellow;
                                    data.SetValue("away");
                                    break;
                                default:
                                    Profile_Status.Background = Brushes.Gray;
                                    data.SetValue("offline");
                                    break;
                            }
                            doc.Save(Directory.GetCurrentDirectory() + @"\Config.xml");
                        }

                        var found = OnlineUsers_Data.FirstOrDefault(x => x.userID == user.userID);
                        if (found != null)
                        {
                            switch (user.Status)
                            {
                                case "Green":
                                    found.Status = "Green";
                                    break;
                                case "Yellow":
                                    found.Status = "Yellow";
                                    break;
                                default:
                                    OnlineUsers_Data.Remove(OnlineUsers_Data.Where(x => x.userID == user.userID).Single());
                                    break;
                            }
                        }
                        else if (found == null && (user.Status == "Green" || user.Status == "Yellow"))
                        {
                            user.Avatar = await ToBitmapImage.Coverter(480, url);
                            OnlineUsers_Data.Add(user);
                        }
                        ListViewUsers.ItemsSource = OnlineUsers_Data;
                        ListViewUsers.Items.Refresh();
                    }
                });
                Connector.connection.On<bool, string, string>("userLeft_Channel", (isLeft, login_id, channelID) =>
                {
                    if (isLeft)
                    {
                        if (UserData.LoginID == login_id && UserData.ChannelID == channelID)
                        {
                            MessagesData.Clear();
                            OnlineUsers_Data.Clear();
                            _ChannelList.Channels.Remove(_ChannelList.Channels.Where(x => x.ChannelHash == UserData.ChannelID).Single());
                            ListViewMessages.Items.Refresh();
                            ListViewUsers.Items.Refresh();
                            ListViewChannels.Items.Refresh();
                            Channel_Name.Text = "";
                            msg.Text = "";
                            WritingUsers_Text.Text = "";
                            UserData.ChannelID = null;
                            Online_Grid.Visibility = Visibility.Collapsed;
                            Launch_Cover.Visibility = Visibility.Visible;
                            msg_grid.SetValue(Grid.ColumnSpanProperty, 2);
                        }
                        else if (UserData.LoginID == login_id && UserData.ChannelID != channelID)
                        {
                            _ChannelList.Channels.Remove(_ChannelList.Channels.Where(x => x.ChannelHash == channelID).Single());
                            ListViewChannels.Items.Refresh();
                        }
                        else if (UserData.LoginID != login_id)
                        {
                            OnlineUsers_Data.Remove(OnlineUsers_Data.Where(x => x.userID == login_id).Single());
                            ListViewUsers.ItemsSource = OnlineUsers_Data;
                            ListViewUsers.Items.Refresh();
                        }
                    }
                });
                Connector.connection.On<bool, string>("Deleted_Message", (isDeleted, messageID) =>
                {
                    if (isDeleted)
                    {
                        MessagesData.Remove(MessagesData.Where(x => x.MessageID == messageID).Single());
                        ListViewMessages.ItemsSource = MessagesData;
                        ListViewMessages.Items.Refresh();
                    }
                });
                Connector.connection.On<bool, string, string>("Edited_Message", (isEdited, messageID, new_text) =>
                {
                    if (isEdited)
                    {
                        MessagesData.Where(x => x.MessageID == messageID).Single().Content = new_text;
                        ListViewMessages.ItemsSource = MessagesData;
                        ListViewMessages.Items.Refresh();
                    }
                });
                Connector.connection.On<bool, string>("Deleted_Channel", (isDeleted, channelID) =>
                {
                    if (isDeleted)
                    {
                        _ChannelList.Channels.Remove(_ChannelList.Channels.Where(x => x.ChannelHash == UserData.ChannelID).Single());
                        if (UserData.ChannelID == channelID)
                        {
                            MessagesData.Clear();
                            OnlineUsers_Data.Clear();
                            ListViewMessages.Items.Refresh();
                            ListViewUsers.Items.Refresh();
                            Channel_Name.Text = "";
                            msg.Text = "";
                            WritingUsers_Text.Text = "";
                            UserData.ChannelID = null;
                            Online_Grid.Visibility = Visibility.Collapsed;
                            Launch_Cover.Visibility = Visibility.Visible;
                            msg_grid.SetValue(Grid.ColumnSpanProperty, 2);
                        }
                        ListViewChannels.Items.Refresh();
                    }
                });
                Connector.connection.On<bool, string, string>("Transfered_channelAdmin", (isTransfered, new_userID, old_userID) =>
                {
                    if (isTransfered)
                    {
                        if (UserData.LoginID == new_userID)
                        {
                            leaveServer_Button.Visibility = Visibility.Hidden;
                        }
                        else if (UserData.LoginID == old_userID)
                        {
                            leaveServer_Button.Visibility = Visibility.Visible;
                        }

                        var found = OnlineUsers_Data.FirstOrDefault(x => x.userID == new_userID);
                        if (found != null)
                        {
                            found.isAdmin = "Visible";
                        }

                        found = OnlineUsers_Data.FirstOrDefault(x => x.userID == old_userID);
                        if (found != null)
                        {
                            found.isAdmin = "Collapsed";
                        }

                        ListViewUsers.ItemsSource = OnlineUsers_Data;
                        ListViewUsers.Items.Refresh();
                    }
                });
                Connector.connection.On<string, string, string>("userBanned_fromChannel", (admin_name, reason, duration) =>
                {
                    Ban_Message_Window banned = new Ban_Message_Window(reason, duration, admin_name);
                    banned.Owner = this;
                    banned.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    Channel_Grid.Opacity = 0.4;
                    Priv_Grid.Opacity = 0.4;
                    msg_grid.Opacity = 0.4;
                    Online_Grid.Opacity = 0.4;
                    banned.Show();

                    banned.Closed += (s, b) =>
                    {
                        Channel_Grid.Opacity = 1;
                        Priv_Grid.Opacity = 1;
                        msg_grid.Opacity = 1;
                        Online_Grid.Opacity = 1;
                        this.Topmost = true;
                        Task.Delay(50);
                        this.Topmost = false;
                    };
                });
                Connector.connection.On<Online_Users_inChannel>("Private_changeStatus", user =>
                {
                    var found = _Private_Users.PrivateUsers_Data.FirstOrDefault(x => x.userID == user.userID);
                    if (found != null)
                    {
                        found.Status = user.Status;
                    }
                    ListViewPrivates.Items.Refresh();
                });
                Connector.connection.On<Private_Users, int>("Checked_privateChannel", async (user, found) =>
                {
                    if (found == 0)
                    {
                        _Private_Users.PrivateUsers_Data.Insert(0, user);
                        ListViewPrivates.Items.Refresh();
                    }

                    await Connector.connection.SendAsync("Switch_Channel", user.channelID, "private");
                });
                Connector.connection.On<bool, ObservableCollection<MessageDataView>, string, List<string>>("Switched_privateChannel", async(isSwitched, messages, channelID, msg_url) =>
                {
                    if (isSwitched)
                    {
                        int i = 0;
                        MessageSender_Data.Clear();
                        await Task.Run(async () => {
                            await this.Dispatcher.BeginInvoke(new ThreadStart(async () =>
                            {
                                foreach (MessageDataView data in messages)
                                {
                                    if (MessageSender_Data.Find(x => x.userID == data.Sender_id) != null)
                                    {
                                        data.Avatar = MessageSender_Data.FirstOrDefault(x => x.userID == data.Sender_id).Avatar;
                                    }
                                    else
                                    {
                                        MessageSender_Handler new_user = new MessageSender_Handler();
                                        new_user.userID = data.Sender_id;
                                        new_user.Avatar = await ToBitmapImage.Coverter(480, msg_url[i]);
                                        data.Avatar = new_user.Avatar;
                                        MessageSender_Data.Add(new_user);
                                    }

                                    if (data.ImageID != null && data.ImageID != String.Empty)
                                    {
                                        data.ImageSource = await ToBitmapImage.Coverter(480, data.ImageID);
                                    }
                                    i++;
                                }
                            }));
                        });

                        Loading_Panel.Visibility = Visibility.Collapsed;
                        MessagesData.Clear();
                        OnlineUsers_Data.Clear();
                        Channel_Name.Text = "#" + _Private_Users.PrivateUsers_Data.FirstOrDefault(x => x.channelID == channelID).Name;
                        UserData.ChannelID = channelID;
                        MessagesData = messages;
                        ListViewUsers.ItemsSource = OnlineUsers_Data;
                        ListViewMessages.ItemsSource = MessagesData;
                        ListViewMessages.Items.MoveCurrentToLast();
                        ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);
                        msg.Opacity = 0.3;
                        msg.Text = "Message " + Channel_Name.Text;
                        WritingUsers_Text.Text = "";
                        msg.IsEnabled = true;
                        Global_Variables.channelType = "private";

                        leaveServer_Button.Visibility = Visibility.Hidden;
                        channelSettings_Button.Visibility = Visibility.Hidden;
                        if (_Private_Users.PrivateUsers_Data.FirstOrDefault(x => x.channelID == channelID).isBlocked == "Unblock User")
                        {
                            msg.IsEnabled = false;
                            msg.Text = "You cannot send message to blocked user";
                        }

                        if (Launch_Cover.Visibility == Visibility.Visible)
                        {
                            Launch_Cover.Visibility = Visibility.Hidden;
                        }
                        Online_Grid.Visibility = Visibility.Collapsed;
                        msg_grid.SetValue(Grid.ColumnSpanProperty, 2);
                        ListViewMessages.Visibility = Visibility.Visible;
                        ListViewMessages.Items.Refresh();
                    }
                });
                Connector.connection.On<string, string>("Blocked_privateUser", (channelID, isBlocked) =>
                {
                    if (UserData.ChannelID == channelID)
                    {
                        if (isBlocked == "Unblock User")
                        {
                            msg.IsEnabled = false;
                            msg.Text = "You cannot send message to blocked user";
                        }
                        else
                        {
                            msg.IsEnabled = true;
                            msg.Text = "Message #" + _Private_Users.PrivateUsers_Data.FirstOrDefault(x => x.channelID == channelID).Name;
                        }
                    }

                    _Private_Users.PrivateUsers_Data.FirstOrDefault(x => x.channelID == channelID).isBlocked = isBlocked;
                    ListViewPrivates.Items.Refresh();
                });
                Connector.connection.On<string, string>("User_changedAvatar", async(userID, url) => 
                {
                    try
                    {
    
                        if (userID == UserData.LoginID)
                        {
                            UserData.Avatar = await ToBitmapImage.Coverter(480, url);
                            Profile_Image.ImageSource = UserData.Avatar;
                        }

                        if (MessageSender_Data.Find(x => x.userID == userID) != null)
                        {
                            MessageSender_Data.Find(x => x.userID == userID).Avatar = await ToBitmapImage.Coverter(480, url);
                        }

                        if (Global_Variables.channelType == "channel")
                        {
                            if (MessageSender_Data.Find(x => x.userID == userID) != null)
                            {
                                OnlineUsers_Data.FirstOrDefault(x => x.userID == userID).Avatar = MessageSender_Data.Find(x => x.userID == userID).Avatar;
                            }
                            else
                            {
                                OnlineUsers_Data.FirstOrDefault(x => x.userID == userID).Avatar = await ToBitmapImage.Coverter(480, url);
                            }
                        }

                        foreach (MessageDataView data in MessagesData.Where(x => x.Sender_id == userID))
                        {
                            data.Avatar = MessageSender_Data.Find(x => x.userID == userID).Avatar;
                        }

                        ListViewUsers.Items.Refresh();
                        ListViewMessages.Items.Refresh();
                    }catch(Exception e) { MessageBox.Show(e.ToString()); }
                });
                Connector.connection.On<string, string>("Private_changedAvatar", async(userID, url) => 
                {
                    _Private_Users.PrivateUsers_Data.Where(x => x.userID == userID).Single().Avatar = await ToBitmapImage.Coverter(480, url);
                    ListViewPrivates.Items.Refresh();
                });
                Connector.connection.On<string, string>("Channel_changedAvatar", async (channelID, url) =>
                {
                    _ChannelList.Channels.FirstOrDefault(x => x.ChannelHash == channelID).Avatar = await ToBitmapImage.Coverter(480, url);
                    ListViewChannels.Items.Refresh();
                });
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
        }
        private void SendMessage_Btn(object sender, RoutedEventArgs e)
        {
            if (msg.Opacity == 1)
            {
                Send_Message.SendMessage(msg.Text);
                msg.Height = 25;
                msg.Text = "";
            }
        }
        private void txtMessage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    msg.AppendText("\n");
                    msg.SelectionStart = msg.Text.Length;
                }
                else
                {
                    Send_Message.SendMessage(msg.Text);
                    msg.Height = 25;
                    msg.Text = "";
                }
            }
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Fullscreen();
            }
            else if (e.ClickCount == 1)
            {
                this.DragMove();
            }
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            m_notifyIcon.Visible = true;
            this.Hide();
        }
        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            Fullscreen();
        }
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            Save_Windowsize.Save(this.ActualWidth, this.ActualHeight);
            Environment.Exit(1);
        }
        private void Download_Update(object sender, RoutedEventArgs e)
        {
            Start_Updater.Start();
        }
        private async void TriggerTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {     
                int lineCount = msg.LineCount;
                if (lineCount > 1 && previousLineCount != lineCount)
                {
                    msg.Height = (lineCount * 19) + 2;
                    previousLineCount = lineCount;
                }
                else if (lineCount == 1 && msg.Text.Length == 0)
                {
                    msg.Height = 25;
                }

                if (msg.Text != String.Empty && Writing == false && msg.Opacity == 1 && msg.IsFocused == true)
                {
                    Writing = true;
                    await Connector.connection.SendAsync("WritingMessage", true);
                }
                else if ((msg.Text == String.Empty || msg.Opacity == 0.3) && Writing == true)
                {
                    await Connector.connection.SendAsync("WritingMessage", false);
                    Writing = false;
                }
            }
            catch (Exception) { }
        }
        private void Find_privateUser(object sender, TextChangedEventArgs e)
        {
            if (Find_privUser.Text.Length == 0)
            {
                ListViewPrivates.ItemsSource = _Private_Users.PrivateUsers_Data;
            }
            else if (Find_privUser.Opacity == 1 && Find_privUser.IsFocused == true)
            {
                ListViewPrivates.ItemsSource = _Private_Users.PrivateUsers_Data.Where(x => x.Name.ToLower().Contains(Find_privUser.Text.ToLower()));
            }
        }
        private void ChannelAdd_Click(object sender, RoutedEventArgs e)
        {
            newChannel = new AddChannel();
            newChannel.Owner = this;
            newChannel.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Channel_Grid.Opacity = 0.4;
            Priv_Grid.Opacity = 0.4;
            msg_grid.Opacity = 0.4;
            Online_Grid.Opacity = 0.4;
            newChannel.Show();

            newChannel.Closed += (s, b) =>
            {
                Channel_Grid.Opacity = 1;
                Priv_Grid.Opacity = 1;
                msg_grid.Opacity = 1;
                Online_Grid.Opacity = 1;
                this.Topmost = true;
                Task.Delay(50);
                this.Topmost = false;
            };
        }
        private async void Clicked_Channel(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                this.Dispatcher.BeginInvoke(new Action(async() =>
                {
                    try
                    {
                        if (Global_Variables.last_clickedChannel_Border != null)
                        {
                            Global_Variables.last_clickedChannel_Border.Visibility = Visibility.Hidden;
                        }

                        Button btn = sender as Button;
                        DockPanel panel = btn.Parent as DockPanel;
                        Border brd = panel.FindName("Selected") as Border;

                        brd.Visibility = Visibility.Visible;

                        Global_Variables.last_clickedChannel_Border = brd;

                        var item = (sender as FrameworkElement).DataContext;
                        channelindex = ListViewChannels.Items.IndexOf(item);

                        if (UserData.ChannelID != _ChannelList.Channels[channelindex].ChannelHash)
                        {
                            Loading_Panel.Visibility = Visibility.Visible;
                            ListViewMessages.Visibility = Visibility.Collapsed;                       
                            WritingUsers_Text.Text = "";
                            msg.Text = ""; ;
                            await Task.Delay(100);
                            await Connector.connection.SendAsync("Switch_Channel", _ChannelList.Channels[channelindex].ChannelHash, "channel");
                        }
                    }
                    catch (Exception) { }
                }));
            });
        }
        private async void Logout_Clicked(object sender, RoutedEventArgs e)
        {
            logged_out = true;
            await Connector.connection.DisposeAsync();
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                XDocument doc = XDocument.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
                var data = doc.Root.Descendants("KeepLogged").FirstOrDefault();
                data.SetValue("False");
                doc.Save(Directory.GetCurrentDirectory() + @"\Config.xml");
                Login_Window loginwindow = new Login_Window();
                loginwindow.Show();
                this.Close();
            });
        }
        private void User_Settings_Clicked(object sender, RoutedEventArgs e)
        {
            User_Settings window = new User_Settings(this.ActualWidth, this.ActualHeight);
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Show();
            this.Hide();
        }
        private async void Channel_Settings(object sender, RoutedEventArgs e)
        {
            try
            {
                await Connector.connection.SendAsync("Check_channelAdmin");
            }
            catch (Exception) { }
        }
        private void Fullscreen()
        {
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);
            if (this.Height == screen.WorkingArea.Height && this.Width == screen.WorkingArea.Width)
            {
                this.Width = MainWindow_Data.Window_Width;
                this.Height = MainWindow_Data.Window_Height;
                this.Top = MainWindow_Data.Window_Top;
                this.Left = MainWindow_Data.Window_Left;
            }
            else
            {
                MainWindow_Data.Window_Height = this.ActualHeight;
                MainWindow_Data.Window_Width = this.ActualWidth;
                MainWindow_Data.Window_Top = this.Top;
                MainWindow_Data.Window_Left = this.Left;

                this.Height = screen.WorkingArea.Height;
                this.Width = screen.WorkingArea.Width;
                this.Top = (screen.WorkingArea.Height - this.Height) / 2 + screen.WorkingArea.Top;
                this.Left = (screen.WorkingArea.Width - this.Width) / 2 + screen.WorkingArea.Left;
            }
        }
        private void Create_ConfirmWindow(string data)
        {
            Confirmation_Window confirm = new Confirmation_Window(data);
            confirm.Owner = this;
            confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Channel_Grid.Opacity = 0.4;
            Priv_Grid.Opacity = 0.4;
            msg_grid.Opacity = 0.4;
            Online_Grid.Opacity = 0.4;
            confirm.Show();

            confirm.Closed += (s, b) =>
            {
                Channel_Grid.Opacity = 1;
                Priv_Grid.Opacity = 1;
                msg_grid.Opacity = 1;
                Online_Grid.Opacity = 1;
            };
        }
        private void Change_Status(object sender, RoutedEventArgs e)
        {
            if (Status_Frame.Visibility == Visibility.Collapsed)
            {
                Status_Frame.Visibility = Visibility.Visible;
            }
            else
            {
                Status_Frame.Visibility = Visibility.Collapsed;
            }
        }
        private void Clicked_OnlineUser(object sender, RoutedEventArgs e)
        {
            panel = sender as StackPanel;
            panel.ContextMenu.PlacementTarget = panel;
            panel.ContextMenu.IsOpen = true;
        }
        private async void Clicked_newStatus(object sender, RoutedEventArgs e)
        {
            try
            {
                Status_Frame.Visibility = Visibility.Collapsed;
                string status = String.Empty;
                var item = (sender as ListView).SelectedIndex;
                switch (item)
                {
                    case 0:
                        status = "online";
                        break;
                    case 1:
                        status = "away";
                        break;
                    default:
                        status = "offline";
                        break;
                }

                await Connector.connection.SendAsync("Change_userStatus", status);
            }
            catch (Exception) { }
        }
        private void MessageText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (msg.Text.Length == 0)
            {
                msg.Text = "Message " + Channel_Name.Text;
                msg.Opacity = 0.3;
            }
        }
        private void MessageText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (msg.Opacity == 0.3)
            {
                msg.Text = "";
                msg.Opacity = 1;
            }
        }
        private void FindConver_Focus(object sender, RoutedEventArgs e)
        {
            if (Find_privUser.Opacity == 0.3)
            {
                Find_privUser.Opacity = 1;
                Find_privUser.Text = "";
            }
        }
        private void FindConver_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Find_privUser.Text.Length == 0)
            {
                Find_privUser.Opacity = 0.3;
                Find_privUser.Text = "Find Conversation";
            }
        }
        private void userSearch_Clicked(object sender, RoutedEventArgs e)
        {
            if (Launch_Cover.Visibility == Visibility.Hidden)
            {
                Online_Grid.Visibility = Visibility.Collapsed;
                Launch_Cover.Visibility = Visibility.Visible;
                msg_grid.SetValue(Grid.ColumnSpanProperty, 2);
                if(Emotes.Visibility == Visibility.Visible)
                {
                    Emotes.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (UserData.ChannelID != null)
                {
                    Launch_Cover.Visibility = Visibility.Hidden;
                    Online_Grid.Visibility = Visibility.Visible;
                    msg_grid.SetValue(Grid.ColumnSpanProperty, 1);
                }
            }
        }
        private void leaveServer_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Create_ConfirmWindow("Leave");
            }
            catch (Exception) { }
        }
        private void Enable_Menu(object sender, MouseEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
            int msgindex = ListViewMessages.Items.IndexOf(item);
            if (MessagesData[msgindex].Sender_id == UserData.LoginID)
            {
                StackPanel msg_panel = sender as StackPanel;
                Border menu = (Border)msg_panel.FindName("msg_Menu");
                menu.Visibility = Visibility.Visible;
            }
        }
        private void Disable_Menu(object sender, MouseEventArgs e)
        {
            StackPanel msg_panel = sender as StackPanel;
            Border menu = (Border)msg_panel.FindName("msg_Menu");
            if (menu != null)
            {
                if (menu.Visibility == Visibility.Visible)
                {
                    menu.Visibility = Visibility.Collapsed;
                }
            }
        }
        private async void Delete_Message(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement).DataContext;
                int msgindex = ListViewMessages.Items.IndexOf(item);
                await Connector.connection.SendAsync("Delete_Message", MessagesData[msgindex].MessageID);
            }
            catch (Exception) { }
        }
        private void Edit_Message(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement).DataContext;
                int msgindex = ListViewMessages.Items.IndexOf(item);
                Button btn = sender as Button;
                StackPanel menu = btn.Parent as StackPanel;
                Border border = menu.Parent as Border;
                StackPanel panel = border.Parent as StackPanel;
                TextBox msg = (TextBox)panel.FindName("Message_Text");
                msg.IsReadOnly = false;
                msg.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#131a20");
                oldmessage = msg.Text;
            }
            catch (Exception) { }
        }
        private async void EditText_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement).DataContext;
                int msgindex = ListViewMessages.Items.IndexOf(item);
                TextBox text = sender as TextBox;

                if (e.Key == Key.Enter)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        text.AppendText("\n");
                        text.SelectionStart = text.Text.Length;
                        ListViewMessages.ScrollIntoView(ListViewMessages.Items.CurrentItem);
                    }
                    else if (text.Text.Length > 0)
                    {
                        text.IsReadOnly = true;
                        text.Background = Brushes.Transparent;
                        text.IsEnabled = false;
                        text.IsEnabled = true;
                        await Connector.connection.SendAsync("Edit_Message", MessagesData[msgindex].MessageID, text.Text);
                        text.Text = oldmessage;
                    }
                }
                else if (e.Key == Key.Escape)
                {
                    text.IsReadOnly = true;
                    text.Background = Brushes.Transparent;
                    text.Text = MessagesData[msgindex].Content;
                    text.IsEnabled = false;
                    text.IsEnabled = true;
                }
            }
            catch (Exception) { }
        }
        private void channelUser_Menu_Open(object sender, RoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            var item = (sender as FrameworkElement).DataContext;
            int index = ListViewUsers.Items.IndexOf(item);
            Global_Variables.Last_userID_onlineMenu = OnlineUsers_Data[index].userID;

            if (leaveServer_Button.Visibility != Visibility.Hidden)
            {
                panel.ContextMenu.Height = 80;
            }

            if (OnlineUsers_Data[index].userID == UserData.LoginID)
            {
                panel.ContextMenu.Visibility = Visibility.Hidden;
            }
        }
        private void Kick_channelUser(object sender, RoutedEventArgs e)
        {
            try
            {
                Create_ConfirmWindow("Kick");
            }
            catch (Exception) { }
        }
        private void Ban_channelUser(object sender, RoutedEventArgs e)
        {
            try
            {
                userBan_Window sett = new userBan_Window();
                sett.Owner = this;
                sett.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                Channel_Grid.Opacity = 0.4;
                Priv_Grid.Opacity = 0.4;
                msg_grid.Opacity = 0.4;
                Online_Grid.Opacity = 0.4;
                sett.Show();

                sett.Closed += (s, b) =>
                {
                    Channel_Grid.Opacity = 1;
                    Priv_Grid.Opacity = 1;
                    msg_grid.Opacity = 1;
                    Online_Grid.Opacity = 1;
                };
            }
            catch (Exception) { }
        }
        private void Transfer_channelAdmin(object sender, RoutedEventArgs e)
        {
            try
            {
                Create_ConfirmWindow("Transfer");
            }
            catch (Exception) { }
        }
        private async void Send_friendRequest(object sender, RoutedEventArgs e)
        {
            await Connector.connection.SendAsync("Send_friendRequest", Global_Variables.Last_userID_onlineMenu);
        }
        private async void Open_privateMessage(object sender, RoutedEventArgs e)
        {
            await Connector.connection.SendAsync("Check_privateChannel", Global_Variables.Last_userID_onlineMenu);
        }
        private async void Switch_privateChannel(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                this.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    var item = (sender as FrameworkElement).DataContext;
                    int index = ListViewPrivates.Items.IndexOf(item);
                    if (UserData.ChannelID != _Private_Users.PrivateUsers_Data[index].channelID)
                    {
                        Loading_Panel.Visibility = Visibility.Visible;
                        ListViewMessages.Visibility = Visibility.Collapsed;
                        await Task.Delay(100);
                        await Connector.connection.SendAsync("Check_privateChannel", _Private_Users.PrivateUsers_Data[index].userID);
                    }
                }));
            });
        }
        private async void Block_privateUser(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
            int index = ListViewPrivates.Items.IndexOf(item);
            await Connector.connection.SendAsync("Block_privateUser", _Private_Users.PrivateUsers_Data[index].channelID, _Private_Users.PrivateUsers_Data[index].userID, _Private_Users.PrivateUsers_Data[index].isBlocked);
        }
        private void Send_Picture(object sender, DragEventArgs e)
        {
            if (UserData.ChannelID != null && UserData.ChannelID != String.Empty)
            {
                if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string data in file)
                {
                    var name = System.IO.Path.GetFileName(data);
                    var ext = System.IO.Path.GetExtension(data);
                    if (ext.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase) || ext.Equals(".png", StringComparison.CurrentCultureIgnoreCase) || ext.Equals(".jpeg", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Preview_Window preview = new Preview_Window(data, name);
                        preview.Owner = this;
                        preview.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                        Channel_Grid.Opacity = 0.4;
                        Priv_Grid.Opacity = 0.4;
                        msg_grid.Opacity = 0.4;
                        Online_Grid.Opacity = 0.4;

                        preview.ShowDialog();

                        Channel_Grid.Opacity = 1;
                        Priv_Grid.Opacity = 1;
                        msg_grid.Opacity = 1;
                        Online_Grid.Opacity = 1;
                        this.Topmost = true;
                        Task.Delay(50);
                        this.Topmost = false;
                    }
                }
            }
        }
        private async void Full_imageOpen(object sender, MouseButtonEventArgs e)
        {
            Image picture = sender as Image;
            var item = (sender as FrameworkElement).DataContext;
            int index = ListViewMessages.Items.IndexOf(item);

            if (picture.Source != null)
            {
                ImageResolution_Window window = new ImageResolution_Window(MessagesData[index].ImageID);
                window.Owner = this;
                window.Width = this.ActualWidth - 100;
                window.Height = this.ActualHeight - 100;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                Channel_Grid.Opacity = 0.4;
                Priv_Grid.Opacity = 0.4;
                msg_grid.Opacity = 0.4;
                Online_Grid.Opacity = 0.4;

                window.ShowDialog();

                Channel_Grid.Opacity = 1;
                Priv_Grid.Opacity = 1;
                msg_grid.Opacity = 1;
                Online_Grid.Opacity = 1;
                this.Topmost = true;
                await Task.Delay(100);
                this.Topmost = false;
            }
        }
        private void Display_Emotes(object sender, RoutedEventArgs e)
        {
            if(Emotes.Visibility == Visibility.Visible)
            {
                Emotes.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (Emotes.Content == null)
                {
                    Emotes.Content = new Emotes_Window();
                }
                Emotes.Visibility = Visibility.Visible;
            }
        }

    }
}
