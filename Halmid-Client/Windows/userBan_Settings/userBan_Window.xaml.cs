using Halmid_Client.Connectors;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Windows;

namespace Halmid_Client.Windows.userBan_Settings
{
    /// <summary>
    /// Logika interakcji dla klasy userBan_Window.xaml
    /// </summary>
    public partial class userBan_Window : Window
    {
        private string[] duration_minutes = new string[] { "30", "60", "120", "300", "720", "1440", "2880", "7200", "10080", "20160", "0" };
        public userBan_Window()
        {
            InitializeComponent();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            this.Hide();
            this.Close();
        }

        private async void Ban_User(object sender, RoutedEventArgs e)
        {
            try
            {
                await Connector.connection.SendAsync("Ban_User", Global_Variables.Last_userID_onlineMenu, Ban_Reason.Text, duration_minutes[combo_duration.SelectedIndex]);
                this.Close();
            }
            catch (Exception) { }
        }
    }
}
