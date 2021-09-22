using Halmid_Client.Variables;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Halmid_Client.Windows.Search_User
{
    /// <summary>
    /// Logika interakcji dla klasy Search_User_Window.xaml
    /// </summary>
    public partial class Search_User_Window : Page
    {
        Friends friend_page;
        searchUser find_user;
        Pending pending_page;

        public Search_User_Window()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            friend_page = new Friends();
            find_user = new searchUser();
            pending_page = new Pending();
            Main_Content.Content = friend_page;
        }
        private void allOnline_Clicked(object sender, RoutedEventArgs e)
        {
            if (Main_Content.Content != friend_page)
            {
                Main_Content.Content = friend_page;
            }
            friend_page.userList.ItemsSource = _Friends.friends.Where(x => x.Status != "Gray");
            friend_page.userList.Items.Refresh();
            friend_page.Friends_Title.Text = "Online Friends";
        }
        private void allFriends_Clicked(object sender, RoutedEventArgs e)
        {
            if (Main_Content.Content != friend_page)
            {
                Main_Content.Content = friend_page;
            }
            friend_page.userList.ItemsSource = _Friends.friends;
            friend_page.userList.Items.Refresh();
            friend_page.Friends_Title.Text = "All Friends";
        }
        private void pendingFriends_Clicked(object sender, RoutedEventArgs e)
        {
            if (Main_Content.Content != pending_page)
            {
                Main_Content.Content = pending_page;
            }
        }
        private void addFriend_Clicked(object sender, RoutedEventArgs e)
        {
            Main_Content.Content = find_user;
        }
    }
}
