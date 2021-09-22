using Halmid_Client.Connectors;
using Halmid_Client.Functions;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Halmid_Client.Windows.Search_User
{
    /// <summary>
    /// Logika interakcji dla klasy Friends.xaml
    /// </summary>
    public partial class Pending : Page
    {


        public Pending()
        {
            InitializeComponent();
            InitConnectors();
            userList.ItemsSource = _Pending.pending;
            userList.Items.Refresh();
        }
        private void InitConnectors()
        {
            Connector.connection.On<Online_Users_inChannel, string>("newPending_friendRequest", async(user,url) =>
            {
                user.Avatar = await ToBitmapImage.Coverter(480, url);
                _Pending.pending.Add(user);
                userList.ItemsSource = _Pending.pending;
                userList.Items.Refresh();
            });
            Connector.connection.On<string>("Accepted_friendRequest", userID =>
            {
                _Pending.pending.Remove(_Pending.pending.Where(x => x.userID == userID).Single());
                userList.ItemsSource = _Pending.pending;
                userList.Items.Refresh();
            });
            Connector.connection.On<string>("Rejected_Pending", userID =>
            {
                _Pending.pending.Remove(_Pending.pending.Where(x => x.userID == userID).Single());
                userList.ItemsSource = _Pending.pending;
                userList.Items.Refresh();
            });
        }
        private async void Add_pendingFriend(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement).DataContext;
                int index = userList.Items.IndexOf(item);
                await Connector.connection.SendAsync("Accept_friendRequest", _Pending.pending[index].userID);
            }
            catch (Exception) { }
        }
        private async void Reject_pendingFriend(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement).DataContext;
                int index = userList.Items.IndexOf(item);
                await Connector.connection.SendAsync("Reject_Pending", _Pending.pending[index].userID);
            }
            catch (Exception) { }
        }
    }
}
