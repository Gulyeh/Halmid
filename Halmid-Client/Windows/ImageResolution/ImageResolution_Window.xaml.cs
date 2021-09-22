using Halmid_Client.Functions;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Halmid_Client.Windows.ImageResolution
{
    /// <summary>
    /// Logika interakcji dla klasy ImageResolution_Window.xaml
    /// </summary>
    public partial class ImageResolution_Window : Window
    {
        string link;

        public ImageResolution_Window(string data)
        {
            InitializeComponent();
            link = data;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Image_Window.Source = await ToBitmapImage.Coverter(1920, link);
            Image_Window.Width = this.ActualWidth;
            Image_Window.Height = this.ActualHeight;
        }
        private void SaveImage(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Title = "Select Destination";
                save.Filter = "PNG File (*.png)|*.png";
                save.DefaultExt = "png";
                if (save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var image = Image_Window.Source;
                    using (var fileStream = new FileStream(save.FileName, FileMode.Create))
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(new Uri(link)));
                        encoder.Save(fileStream);
                    }
                }
            }
            catch (Exception) { }
        }
        private void CopyImage(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Clipboard.SetImage((BitmapSource)Image_Window.Source);
            }
            catch (Exception) { }
        }
        private void CopyLink(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(link);
        }
        private void OpenLink(object sender, RoutedEventArgs e)
        {
            Process.Start(link);
        }
        private void Close_Preview(object sender, RoutedEventArgs e)
        {
            Image_Window.Source = null;
            this.Close();
        }
    }
}
