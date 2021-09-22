using Halmid_Client.Connectors;
using Halmid_Client.Functions;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace Halmid_Client.Windows.Search_User
{
    /// <summary>
    /// Logika interakcji dla klasy searchUser.xaml
    /// </summary>
    public partial class searchUser : Page
    {
        List<Online_Users_inChannel> found_users = new List<Online_Users_inChannel>();
        Timer timer;
        Timer timer2;
        Button addFriend;
        TextBlock text;
        int index;

        public searchUser()
        {
            InitializeComponent();
            InitConnectors();

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += OnElapsed;

            timer2 = new Timer();
            timer2.Interval = 3000;
            timer2.Elapsed += HideMessage;
        }

        private void InitConnectors()
        {
            Connector.connection.On<List<Online_Users_inChannel>, int, List<string>>("foundSearch", async (users, amount, urls) =>
            {
                if (amount > 0)
                {
                    for(int i=0; i < urls.Count; i++)
                    {
                        users[i].Avatar = await ToBitmapImage.Coverter(480, urls[i]);
                    }

                    found_users = users;
                    usersFound_List.ItemsSource = found_users;
                }
                else
                {
                    found_users.Clear();
                    usersFound_List.ItemsSource = found_users;
                    usersFound_List.Items.Refresh();
                }
            });
            Connector.connection.On<bool>("Sent_friendRequest", isReceived =>
            {
                DockPanel panel = addFriend.Parent as DockPanel;
                text = (TextBlock)panel.FindName("addFriend_Response");
                timer2.Start();
                if (isReceived)
                {
                    text.Text = "Request sent";
                }
                else
                {
                    text.Text = "Already sent or you are friend with this user";
                }
            });
        }
        private void HideMessage(object source, ElapsedEventArgs e)
        {
            text.Text = "";
            usersFound_List.Items.Refresh();
            timer2.Stop();
        }
        private void findUser(object sender, RoutedEventArgs e)
        {
            if (msg.Text.Length > 0 && timer.Enabled == false)
            {
                Loading_Spinner.Visibility = Visibility.Visible;
            }
            else if (msg.Text.Length == 0)
            {
                Loading_Spinner.Visibility = Visibility.Collapsed;
                timer.Stop();
                return;
            }
            timer.Stop();
            timer.Start();
        }
        private async void OnElapsed(object source, ElapsedEventArgs e)
        {
            await Dispatcher.Invoke(() => Connector.connection.SendAsync("Find_user", msg.Text));
            timer.Stop();
            Dispatcher.Invoke(() => Loading_Spinner.Visibility = Visibility.Collapsed);
        }
        private async void Add_Friend(object sender, RoutedEventArgs e)
        {
            addFriend = sender as Button;
            var item = (sender as FrameworkElement).DataContext;
            index = usersFound_List.Items.IndexOf(item);
            await Connector.connection.SendAsync("Send_friendRequest", found_users[index].userID);
        }
        private async void privateMessage_user(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
            int index = usersFound_List.Items.IndexOf(item);
            await Connector.connection.SendAsync("Check_privateChannel", found_users[index].userID);
        }
    }
}
