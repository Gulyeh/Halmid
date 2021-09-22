using System.Text.RegularExpressions;

namespace Halmid_Client.Chat_Functions
{
    class Check_Emoji
    {
        public static string CheckEmoji(string text)
        {
            if (Regex.IsMatch(text, "<*>"))
            {
                string icon = text.Split('<', '>')[1];
            }
            return null;
        }
    }
}
