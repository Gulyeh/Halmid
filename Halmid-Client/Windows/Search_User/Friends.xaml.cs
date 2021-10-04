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
    /// Logika interakcji dla klasy _Friends.friends.xaml
    /// </summary>
    public partial class Friends : Page
    {
        public Friends()
        {
            InitializeComponent();
            InitConnectors();
            userList.ItemsSource = _Friends.friends.Where(x => x.Status != "Gray");
            userList.Items.Refresh();
        }

        private void InitConnectors()
        {
            Connector.connection.On<string>("Deleted_asFriend", userID =>
            {
                try
                {
                    if (_Friends.friends.FirstOrDefault(x => x.userID == userID) != null)
                    {
                        _Friends.friends.Remove(_Friends.friends.Where(x => x.userID == userID).Single());
                    }


                    if (Friends_Title.Text == "All Friends")
                    {
                        userList.ItemsSource = _Friends.friends;
                        userList.Items.Refresh();
                    }
                    else
                    {
                        userList.ItemsSource = _Friends.friends.Where(x => x.Status != "Gray");
                        userList.Items.Refresh();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            });
            Connector.connection.On<Online_Users_inChannel, string>("new_friendAdded", async(user, url)=>
            {
                try
                {
                    user.Avatar = await ToBitmapImage.Coverter(480, url);
                    _Friends.friends.Add(user);
                    if (Friends_Title.Text == "All Friends")
                    {
                        userList.ItemsSource = _Friends.friends;
                    }
                    else
                    {
                        userList.ItemsSource = _Friends.friends.Where(x => x.Status != "Gray");
                    }
                    userList.Items.Refresh();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            });
            Connector.connection.On<Online_Users_inChannel>("Friend_Online", user =>
            {
                try
                {
                    if (_Friends.friends.FirstOrDefault(x => x.userID == user.userID) != null)
                    {
                        _Friends.friends.FirstOrDefault(x => x.userID == user.userID).Status = user.Status;
                        NotifyBallon.Show(_Friends.friends.FirstOrDefault(x => x.userID == user.userID).Name + " is Online", "Friend Online");
                    }

                    if (Friends_Title.Text == "All Friends")
                    {
                        userList.ItemsSource = _Friends.friends;
                        userList.Items.Refresh();
                    }
                    else
                    {
                        userList.ItemsSource = _Friends.friends.Where(x => x.Status != "Gray");
                        userList.Items.Refresh();
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            });
            Connector.connection.On<Online_Users_inChannel>("Friend_changedStatus", user =>
            {
                if(user.Status == "Gray")
                {
                    if (_Friends.friends.FirstOrDefault(x => x.userID == user.userID) != null)
                    {
                        _Friends.friends.FirstOrDefault(x => x.userID == user.userID).Status = "Gray";
                    }

                    if (Friends_Title.Text == "All Friends")
                    {
                        userList.ItemsSource = _Friends.friends;
                    }
                    else
                    {
                        userList.ItemsSource = _Friends.friends.Where(x => x.Status != "Gray");
                    }
                }
                else
                {
                    if (_Friends.friends.FirstOrDefault(x => x.userID == user.userID) != null)
                    {
                        _Friends.friends.FirstOrDefault(x => x.userID == user.userID).Status = user.Status;
                        userList.ItemsSource = _Friends.friends;
                    }
                }
                userList.Items.Refresh();
            });
            Connector.connection.On<string, string>("Friend_changedAvatar", async (userID, url) => 
            { 
                _Friends.friends.Where(x => x.userID == userID).Single().Avatar = await ToBitmapImage.Coverter(480, url);
                _Pending.pending.Where(x => x.userID == userID).Single().Avatar = await ToBitmapImage.Coverter(480, url);
                userList.Items.Refresh();
            });
        }
        private async void Delete_Friend(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = (sender as FrameworkElement).DataContext;
                int index = userList.Items.IndexOf(item);
                await Connector.connection.SendAsync("Delete_Friend", _Friends.friends[index].userID);
            }
            catch (Exception) { }
        }
        private async void privateMessage_user(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext;
            int index = userList.Items.IndexOf(item);
            await Connector.connection.SendAsync("Check_privateChannel", _Friends.friends[index].userID);
        }
    }
}
