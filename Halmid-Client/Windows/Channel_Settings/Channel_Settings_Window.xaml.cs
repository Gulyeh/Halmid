using Halmid_Client.Connectors;
using Halmid_Client.Variables;
using Halmid_Client.Windows.Confirmation;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Halmid_Client.Windows.Channel_Settings
{
    /// <summary>
    /// Logika interakcji dla klasy Channel_Settings_Window.xaml
    /// </summary>
    public partial class Channel_Settings_Window : Window
    {
        bool show_confirmation = false;

        public Channel_Settings_Window()
        {
            InitializeComponent();
            FillHash();
            this.ShowInTaskbar = false;
        }

        private async void CopyHash(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Invite_Hash_Text.Text);
            Clipboard_Text.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            Clipboard_Text.Visibility = Visibility.Hidden;
        }
        private void FillHash()
        {
            Invite_Hash_Text.Text = UserData.ChannelID;
        }
        protected override void OnDeactivated(EventArgs e)
        {
            if (show_confirmation == false)
            {
                base.OnDeactivated(e);
                this.Hide();
                this.Close();
            }
        }
        private async void Change_Avatar(object sender, RoutedEventArgs e)
        {
            try
            {
                show_confirmation = true;
                System.Windows.Forms.OpenFileDialog Avatar = new System.Windows.Forms.OpenFileDialog();
                Avatar.Title = "Select Server Avatar";
                Avatar.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png";
                System.Windows.Forms.DialogResult result = Avatar.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    string ImageID = Functions.RandomChars.GetRandom();
                    await Functions.PostToApi.SendtoApi(ImageID, Avatar.FileName, "Channel_Avatar");
                    await Connector.connection.SendAsync("Channel_changeAvatar", ImageID);
                }
                show_confirmation = false;
            }
            catch (Exception) { }
        }
        private async void Change_Name(object sender, RoutedEventArgs e)
        {
            try
            {
                if (New_Name_Text.Text.Length > 3)
                {
                    Name_Status.Text = "";
                    New_Name_Panel.IsEnabled = false;
                    var found = _ChannelList.Channels.FirstOrDefault(x => x.ChannelHash == UserData.ChannelID);
                    string oldchannel_name = found.Name;

                    await Connector.connection.SendAsync("Change_channel_Name", New_Name_Text.Text);
                    await Task.Delay(1000);

                    found = _ChannelList.Channels.FirstOrDefault(x => x.ChannelHash == UserData.ChannelID);

                    if (oldchannel_name != found.Name)
                    {
                        Name_Status.Text = "Updated Successfuly";
                        New_Name_Text.Text = "";
                        await Task.Delay(3000);
                        Name_Status.Text = "";
                    }
                    else
                    {
                        Name_Status.Text = "Update failed";
                        await Task.Delay(3000);
                        Name_Status.Text = "";
                    }
                    New_Name_Panel.IsEnabled = true;
                }
                else
                {
                    Name_Status.Text = "Name is too short - 3 characters are required";
                    await Task.Delay(3000);
                    Name_Status.Text = "";
                }
            }
            catch (Exception) { }
        }
        private void Delete_Server(object sender, RoutedEventArgs e)
        {
            try
            {
                show_confirmation = true;
                Confirmation_Window confirm = new Confirmation_Window("Delete");
                confirm.Owner = this;
                confirm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                this.Hide();
                confirm.Show();
            }
            catch (Exception) { }
        }
        private void Unban_List(object sender, RoutedEventArgs e)
        {
            try
            {
                show_confirmation = true;
                UnbanList_Window list = new UnbanList_Window(this.Owner.ActualHeight);
                list.Owner = this;
                list.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                list.Show();
                this.Hide();
            }
            catch (Exception) { }
        }
    }
}
