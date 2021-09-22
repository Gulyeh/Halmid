using Halmid_Client.Windows.Loading;
using System.Net;
using System.Threading.Tasks;

namespace Halmid_Client.Functions
{
    class Check_Connection
    {
        public static void Check_InternetConnection(MainWindow main)
        {
            Task.Run(async () =>
            {
                bool droppedConnection = false;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("www.google.com");
                request.Timeout = 5000;
                request.Credentials = CredentialCache.DefaultNetworkCredentials;

                while (!droppedConnection)
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        droppedConnection = true;
                        Loading_Window loading = new Loading_Window("Reconnect");
                        loading.Show();
                        main.Close();
                    }
                    await Task.Delay(8000);
                }
            });
        }
    }
}
