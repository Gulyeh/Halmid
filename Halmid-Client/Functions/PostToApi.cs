using Halmid_Client.Variables;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Halmid_Client.Functions
{
    class PostToApi
    {
        private class PayLoad1
        {
            public string channelID { get; set; }
            public string ImageID { get; set; }
            public string Base64 { get; set; }
            public string Post_Data { get; set; }
        }
        private class PayLoad2
        {
            public string userID { get; set; }
            public string ImageID { get; set; }
            public string Base64 { get; set; }
            public string Post_Data { get; set; }
        }
        private class PayLoad3
        {
            public string ImageID { get; set; }
            public string Base64 { get; set; }
            public string Post_Data { get; set; }
        }

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        public static async Task SendtoApi(string imageid, string path, string type)
        {
            try
            {
                string base64;
                string stringPayload = String.Empty;

                if (path != String.Empty)
                {
                    base64 = Convert.ToBase64String(File.ReadAllBytes(path));
                }
                else
                {
                    base64 = Convert.ToBase64String(ImageToByte(Clipboard.GetImage()));
                }

                if (type == "Message")
                {
                    PayLoad1 data = new PayLoad1
                    {
                        channelID = UserData.ChannelID,
                        ImageID = imageid,
                        Base64 = base64,
                        Post_Data = type
                    };
                    stringPayload = JsonConvert.SerializeObject(data);
                }
                else if (type == "User_Avatar")
                {
                    PayLoad2 data = new PayLoad2
                    {
                        userID = UserData.LoginID,
                        ImageID = imageid,
                        Base64 = base64,
                        Post_Data = type
                    };
                    stringPayload = JsonConvert.SerializeObject(data);
                }
                else if (type == "Channel_Avatar")
                {
                    PayLoad3 data = new PayLoad3
                    {
                        ImageID = imageid,
                        Base64 = base64,
                        Post_Data = type
                    };
                    stringPayload = JsonConvert.SerializeObject(data);
                }

                var httpClient = new HttpClient();
                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                await httpClient.PostAsync("http://31.178.21.16:7345/api/UploadImage", httpContent);
            }
            catch (Exception) { }
        }
    }
}
