using Halmid_Client.Connectors;
using Halmid_Client.Functions;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Halmid_Client.Windows.Channel_Settings
{
    /// <summary>
    /// Logika interakcji dla klasy UnbanList_Window.xaml
    /// </summary>
    public partial class UnbanList_Window : Window
    {
        List<Online_Users_inChannel> found_users = new List<Online_Users_inChannel>();
        IDisposable con1;
        IDisposable con2;

        public UnbanList_Window(double window_height)
        {
            InitializeComponent();
            this.Height = window_height - 150;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            con1 = Connector.connection.On<List<Online_Users_inChannel>, int, List<string>>("Got_Banlist", async (users, amount, urls) =>
            {
                if (amount > 0)
                {
                    int i = 0;
                    foreach(Online_Users_inChannel data in users)
                    {
                        data.Avatar = await ToBitmapImage.Coverter(480, urls[i]);
                        i++;
                    }
                    found_users = users;
                    usersFound_List.ItemsSource = found_users;
                }
            });
            con2 = Connector.connection.On<bool, string>("Unbanned_user", (isUnbanned, userID) =>
            {
                if (isUnbanned)
                {
                    found_users.Remove(found_users.Where(x => x.userID == userID).Single());
                    usersFound_List.ItemsSource = found_users;
                    usersFound_List.Items.Refresh();
                }
            });
            await Connector.connection.SendAsync("Get_Banlist");
        }
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            con1.Dispose();
            con2.Dispose();
            this.Hide();
            this.Owner.Close();
            this.Close();
        }
        private async void Unban_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement).DataContext;
                int index = usersFound_List.Items.IndexOf(item);
                await Connector.connection.SendAsync("Unban_user", found_users[index].userID);
            }
            catch (Exception) { }
        }
    }
}
