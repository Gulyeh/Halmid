using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Halmid_Client.Functions
{
    class ToBitmapImage
    {
        public static Task<BitmapImage> Coverter(int quality, string uri)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
                bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = quality;
                bitmap.EndInit();
                bitmap.DownloadCompleted += (s, d) => 
                {
                    bitmap.Freeze();
                };
                return Task.FromResult(bitmap);
            }catch(Exception e) { MessageBox.Show(e.ToString()); return null; }
        }
    }
}
