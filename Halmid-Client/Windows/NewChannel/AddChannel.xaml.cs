using Halmid_Client.Connectors;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Halmid_Client.Windows.NewChannel
{
    /// <summary>
    /// Logika interakcji dla klasy AddChannel.xaml
    /// </summary>
    public partial class AddChannel : Window
    {

        public BitmapImage UploadImage = new BitmapImage();
        bool ClickedAvatar = false;
        OpenFileDialog Avatar;

        public AddChannel()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UploadImage.BeginInit();
            UploadImage.UriSource = new Uri("pack://application:,,,/Images/Avatars/logo.png", UriKind.Absolute);
            UploadImage.EndInit();
            ServerImage.ImageSource = UploadImage;
        }
        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            if(this.ClickedAvatar == false)
            {
                this.Hide();
                this.Close();
            }
        }
        private void SetAvatar_Click(object sender, RoutedEventArgs e)
        {
            ClickedAvatar = true;
            Avatar = new OpenFileDialog();
            Avatar.Title = "Select Server Avatar";
            Avatar.Filter = "jpg files (*.jpg)|*.jpg|png files (*.png)|*.png";
            DialogResult result = Avatar.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ServerImage.ImageSource = new BitmapImage(new Uri(Avatar.FileName));
            }
            else
            {
                Avatar = null;
            }
            ClickedAvatar = false;
        }  
        private async void NewChannel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Channel_Name.Text.Length > 0)
                {
                    Create_err_msg.Text = "";
                    Channel_Name.IsEnabled = false;
                    Create_Button.IsEnabled = false;
                    string ImageID = "default";
                    if (Avatar != null)
                    {
                        ImageID = Functions.RandomChars.GetRandom();
                        await Functions.PostToApi.SendtoApi(ImageID, Avatar.FileName, "Channel_Avatar");
                    }
                    System.Windows.Forms.MessageBox.Show("ok2");
                    await Connector.connection.SendAsync("Create_Channel", Channel_Name.Text, ImageID);
                    await Task.Delay(1000);
                    Channel_Name.IsEnabled = true;
                    Create_Button.IsEnabled = true;
                }
            }
            catch (Exception ed)
            {
                System.Windows.Forms.MessageBox.Show(ed.ToString());
                Create_err_msg.Text = "Cannot create new server";
                Channel_Name.IsEnabled = true;
                Create_Button.IsEnabled = true;
            }
        }
        private async void Join_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                wrong_text.Visibility = Visibility.Hidden;
                if (server_id.Text.Length > 0)
                {
                    Join_Button.IsEnabled = false;
                    server_id.IsEnabled = false;
                    await Connector.connection.SendAsync("Join_Channel", server_id.Text);
                }
            }
            catch (Exception)
            {
                Join_Button.IsEnabled = true;
                server_id.IsEnabled = true;
            }
        }
    }
}
