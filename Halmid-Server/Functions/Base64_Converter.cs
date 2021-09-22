using System;
using System.IO;
using System.Linq;

namespace Halmid_Server.Functions
{
    public class Base64_Converter
    {
        public static char[] Convert_Data(string base64, string channelID)
        {
            try
            {
                Random random = new Random();
                const string chars = "abcdefghijklmnoprstuwxyz0123456789";

                char[] imageid = (Enumerable.Repeat(chars, 12)
                  .Select(s => s[random.Next(s.Length)]).ToArray());

                byte[] imageBytes = Convert.FromBase64String(base64);
                File.WriteAllBytes("C:/xampp/htdocs/" + channelID + "/" + imageid + ".png", imageBytes);
                return imageid;
            }
            catch (Exception) { return null; }
        }
    }
}
