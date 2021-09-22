using Halmid_Client.Chat_Functions;
using Halmid_Client.Functions;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Halmid_Client.Windows.ImagePreview
{
    /// <summary>
    /// Logika interakcji dla klasy Preview_Window.xaml
    /// </summary>
    public partial class Preview_Window : Window
    {
        string path;
        string name;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public Preview_Window(string filepath, string filename)
        {
            InitializeComponent();
            path = filepath;
            name = filename;
            InitData();
        }

        private void InitData()
        {
            if (path != String.Empty)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmap.UriSource = new Uri(path);
                bitmap.DecodePixelWidth = 1280;
                bitmap.EndInit();
                bitmap.DownloadCompleted += (s, d) =>
                {
                    bitmap.Freeze();
                };
                ImagePreview.Source = bitmap;
                ImagePreview.Source.Freeze();
                bitmap = null;
                FileName.Text = name;
            }
            else
            {
                ImagePreview.Source = GetClipboardImage();
            }
        }

        private BitmapImage GetClipboardImage()
        {
            BitmapSource bitmapSource = Clipboard.GetImage();

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.CacheOption = BitmapCacheOption.OnLoad;
            bImg.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bImg.DecodePixelWidth = 1280;
            bImg.EndInit();
            bImg.DownloadCompleted += (s, d) =>
            {
                bImg.Freeze();
            };
            memoryStream.Close();

            return bImg;
        }
        private async void Upload_Message(object sender, RoutedEventArgs e)
        {
            string imageID = RandomChars.GetRandom();
            await PostToApi.SendtoApi(imageID, path, "Message");
            Send_Message.Send_ImageMessage(msg.Text, imageID);
            this.Close();
        }
        private void Cancel_Upload(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
