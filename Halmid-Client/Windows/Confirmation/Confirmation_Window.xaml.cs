using Halmid_Client.Connectors;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Windows;

namespace Halmid_Client.Windows.Confirmation
{
    /// <summary>
    /// Logika interakcji dla klasy Confirmatio_Window.xaml
    /// </summary>
    public partial class Confirmation_Window : Window
    {
        string command;
        public Confirmation_Window(string data)
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            command = data;
        }
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            this.Hide();
            if (command == "Delete")
            {
                this.Owner.Close();
            }
            this.Close();
        }
        private async void Confirm_Button(object sender, RoutedEventArgs e)
        {
            this.Hide();
            switch (command)
            {
                case "Leave":
                    await Connector.connection.SendAsync("Leave_Channel");
                    break;
                case "Delete":
                    await Connector.connection.SendAsync("Delete_Channel", UserData.ChannelID);
                    break;
                case "Kick":
                    await Connector.connection.SendAsync("Kick_User", Global_Variables.Last_userID_onlineMenu);
                    break;
                case "Transfer":
                    await Connector.connection.SendAsync("Transfer_channelAdmin", Global_Variables.Last_userID_onlineMenu);
                    break;
                default:
                    break;
            }
            if (command == "Delete")
            {
                this.Owner.Close();
            }
            this.Close();
        }
        private void Deny_Button(object sender, RoutedEventArgs e)
        {
            this.Hide();
            if (command == "Delete")
            {
                this.Owner.Close();
            }
            this.Close();
        }
    }
}
