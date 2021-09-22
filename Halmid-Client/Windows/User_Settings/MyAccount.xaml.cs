using Halmid_Client.Connectors;
using Halmid_Client.Functions;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Halmid_Client.Windows.User_Settings
{
    /// <summary>
    /// Logika interakcji dla klasy MyAccount.xaml
    /// </summary>
    public partial class MyAccount : Page
    {
        public MyAccount()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Nick.Text = UserData.Name;
            Profile_Image.ImageSource = UserData.Avatar;
            Username_text.Text = UserData.Username;
            Loginid_Text.Text = UserData.LoginID;
            Channels_text.Text = "Currently in " + _ChannelList.Channels.Count().ToString() + " channels";
        }
        private async void Apply_Nick(object sender, RoutedEventArgs e)
        {

            if (New_Nick_Text.Text.Length > 0 && UserData.Name != New_Nick_Text.Text)
            {
                New_Nick_Panel.IsEnabled = false;
                Nick_Status.Text = "";
                await Connector.connection.SendAsync("Change_user_Nick", New_Nick_Text.Text);
                await Task.Delay(1000);
                if (New_Nick_Text.Text == UserData.Name)
                {
                    Nick.Text = UserData.Name;
                    Nick_Status.Text = "Updated Successfuly";
                    New_Nick_Text.Text = "";
                    await Task.Delay(3000);
                    Nick_Status.Text = "";
                }
                else
                {
                    Nick_Status.Text = "Update failed";
                    await Task.Delay(3000);
                    Nick_Status.Text = "";
                }
                New_Nick_Panel.IsEnabled = true;
            }
            else
            {
                Nick_Status.Text = "New username is same as old one";
                await Task.Delay(3000);
                Nick_Status.Text = "";
            }
        }
        private async void Apply_Login(object sender, RoutedEventArgs e)
        {
            if (Login_Field.Text.Length >= 5)
            {
                New_Login_Panel.IsEnabled = false;
                Login_Status.Text = "";

                string username = toSha256.sha256(Login_Field.Text);

                await Connector.connection.SendAsync("Change_user_Login", username);
                await Task.Delay(2000);
                if (username == UserData.Username)
                {
                    Username_text.Text = UserData.Username;
                    Login_Status.Text = "Updated Successfuly";
                    Login_Field.Text = "";
                    await Task.Delay(3000);
                    Login_Status.Text = "";
                }
                else
                {
                    Login_Status.Text = "Update failed";
                    await Task.Delay(3000);
                    Login_Status.Text = "";
                }
                New_Login_Panel.IsEnabled = true;
            }
            else
            {
                if (Login_Field.Text == UserData.LoginID)
                {
                    Login_Status.Text = "New login is same as old one";
                    await Task.Delay(3000);
                    Login_Status.Text = "";
                }
                else
                {
                    Login_Status.Text = "Login is too short - more than 5 characters is requested!";
                    await Task.Delay(3000);
                    Login_Status.Text = "";
                }

            }
        }
        private async void Change_Avatar(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog Avatar = new OpenFileDialog();
                Avatar.Title = "Select Server Avatar";
                Avatar.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png";
                DialogResult result = Avatar.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string ImageID = Functions.RandomChars.GetRandom();
                    await Functions.PostToApi.SendtoApi(ImageID, Avatar.FileName, "User_Avatar");
                    await Connector.connection.SendAsync("User_changeAvatar", ImageID);
                    await Task.Delay(2000);
                    Profile_Image.ImageSource = UserData.Avatar;
                }
            }
            catch (Exception) { }
        }
    }
}
