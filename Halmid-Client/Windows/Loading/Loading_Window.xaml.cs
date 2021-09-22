using Halmid_Client.Connectors;
using Halmid_Client.Functions;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Halmid_Client.Windows.Loading
{
    /// <summary>
    /// Logika interakcji dla klasy Loading_Window.xaml
    /// </summary>
    public partial class Loading_Window : Window
    {
        bool open = false;
        bool got_name;
        bool got_channels;
        bool got_friends;
        bool got_privates;
        bool got_pending;
        IDisposable con1;
        IDisposable con2;
        IDisposable con3;
        IDisposable con4;
        IDisposable con5;

        public Loading_Window(string type)
        {
            InitializeComponent();
            InitConnectors();
            Use_Function(type);
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private void Use_Function(string data)
        {
            switch (data)
            {
                case "Reconnect":
                    Reconnect();
                    break;
                case "Fill_Data":
                    Fill_Data();
                    break;
                default:
                    break;
            }
        }
        private void InitConnectors()
        {
            con1 = Connector.connection.On<string, string>("Receive_UserData", async(name, avatar) =>
            {
                UserData.Name = name;
                UserData.Avatar = await ToBitmapImage.Coverter(640, avatar);
                got_name = true;
            });

            con2 = Connector.connection.On<ObservableCollection<ChannelList>, List<string>>("Receive_UserChannels", async(list, url) =>
            {
                for (int i = 0; i < url.Count; i++)
                {
                    list[i].Avatar = await ToBitmapImage.Coverter(480, url[i]);
                }

                _ChannelList.Channels = list;
                got_channels = true;
            });

            con3 = Connector.connection.On<List<Online_Users_inChannel>, List<string>>("get_Friends", async(user, url) =>
            {
                for (int i = 0; i < url.Count; i++)
                {
                    user[i].Avatar = await ToBitmapImage.Coverter(480, url[i]);
                }
                _Friends.friends = user;
                got_friends = true;
            });

            con4 = Connector.connection.On<List<Online_Users_inChannel>, List<string>>("Pending_friendRequests", async(user, url) =>
            {
                for (int i = 0; i < url.Count; i++)
                {
                    user[i].Avatar = await ToBitmapImage.Coverter(480, url[i]);
                }

                _Pending.pending = user;
                got_pending = true;
            });

            con5 = Connector.connection.On<ObservableCollection<Private_Users>, List<string>>("Got_UserPrivates", async(user, url) =>
            {
                for (int i = 0; i < url.Count; i++)
                {
                    user[i].Avatar = await ToBitmapImage.Coverter(480, url[i]);
                }
                _Private_Users.PrivateUsers_Data = user;
                got_privates = true;
            });
        }
        private void ReadConfig()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
                if (doc.SelectSingleNode("//Config/UserData/Status").InnerText != String.Empty)
                {
                    Global_Variables.status = doc.SelectSingleNode("//Config/UserData/Status").InnerText;
                }
            }
            catch (Exception e) { MessageBox.Show(e.ToString());}
        }
        private async void Fill_Data()
        {
            got_name = false;
            got_channels = false;
            Task_Msg.Text = "Getting data from server...";

            await GetData_Server.Receive_Data();
            await Task.Run(async () =>
            {
                do
                {
                    if (got_name == true && got_channels == true && got_friends == true && got_pending == true && got_privates == true)
                    {
                        await this.Dispatcher.BeginInvoke(new Action(() =>
                         {
                             Hide();
                             if (!open)
                             {
                                 open = true;
                                 MainWindow main = new MainWindow();
                                 Check_Connection.Check_InternetConnection(main);
                                 main.Show();
                             }
                             Close();
                         }));
                    }
                    await Task.Delay(100);
                } while (this.IsVisible == true);
            });

        }
        private async void Reconnect()
        {
            Task_Msg.Text = "Reconnecting to server...";
            do
            {
                try
                {
                    ReadConfig();
                    await Connector.connection.DisposeAsync();
                    Create_HubConnection.NewConnection();
                    await Connect_Server.Connect();
                    await Connector.connection.SendAsync("Reconnect", UserData.LoginID, Global_Variables.status, UserData.ChannelID);
                    await GetData_Server.Receive_Data();

                    Application.Current.Dispatcher.Invoke((Action)delegate
                    {
                        if (!open)
                        {
                            open = true;
                            MainWindow main = new MainWindow();
                            Check_Connection.Check_InternetConnection(main);
                            main.Show();
                        }
                    });
                    break;
                }
                catch (Exception)
                {
                }
                await Task.Delay(3000);
            } while (1 == 1);

            Application.Current.Dispatcher.Invoke(() => { this.Close(); });
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }
        private void Window_onClosing(object sender, CancelEventArgs e)
        {
            con1.Dispose();
            con2.Dispose();
            con3.Dispose();
            con4.Dispose();
            con5.Dispose();
        }
    }
}
