using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace Halmid_Updater
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(3000);
            startDownload();
        }
        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        private async void startDownload()
        {
            try
            {
                Info_Progress.Text = "Downloading...";
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += client_DownloadProgressChanged;
                    wc.DownloadFileCompleted += client_DownloadFileCompleted;
                    await wc.DownloadFileTaskAsync(
                        new Uri("http://31.178.21.16/Update/Update.zip"),
                        AppDomain.CurrentDomain.BaseDirectory + @"Update_Pack.zip"
                    );
                }
            }
            catch (Exception)
            {
                Info_Progress.Text = "Error while downloading";
                await Task.Delay(1500);
                Info_Progress.Text = "Renewing Download";
                await Task.Delay(1500);
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    Download_Progress.Value = 0;
                });
                startDownload();
            }
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                Download_Progress.Value = e.ProgressPercentage;
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try{
                Application.Current.Dispatcher.Invoke((Action)delegate
                {
                    if(Download_Progress.Value == 100)
                    {
                        Download_Progress.Value = 0;
                        Info_Progress.Text = "Extracting...";
                        await Functions.ExtractionFiles.Extract();
                        await Task.Delay(1000);
                        ProcessStartInfo start = new ProcessStartInfo();
                        start.FileName = AppDomain.CurrentDomain.BaseDirectory + @"Halmid-Client.exe";
                        Process.Start(start);
                        this.Close();
                    }
                });
            }catch(Exception) { this.Close(); }
        }
    }
}
