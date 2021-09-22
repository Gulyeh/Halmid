using Halmid_Client.Connectors;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace Halmid_Client.Chat_Functions
{
    class Send_Message
    {
        public static async void SendMessage(string msg)
        {
            try
            {
                if (msg != String.Empty)
                {
                    await Connector.connection.SendAsync("SendMessage", msg, DateTime.Now.ToString("dd/MM/yyyy - H:mm"), Global_Variables.channelType);
                }
            }
            catch (Exception) { }
        }
        public static async void Send_ImageMessage(string msg, string imageID)
        {
            try
            {
                await Connector.connection.SendAsync("Send_ImageMessage", imageID, msg, DateTime.Now.ToString("dd/MM/yyyy - H:mm"), Global_Variables.channelType);
            }
            catch (Exception) { }
        }
    }
}
