using Halmid_Client.Connectors;
using Halmid_Client.Functions;
using Halmid_Client.Variables;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

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
                    var key = new AesManaged().Key;
                    var vector = new AesManaged().IV;
                    var crypted = CryptMessage.Encrypt(msg, key, vector);
                    var keypair = new Dictionary<string, string>
                    {
                        {"key", Convert.ToBase64String(key) + "!" + Convert.ToBase64String(vector)},
                        {"message", Convert.ToBase64String(crypted)}
                    };
                    await Connector.connection.SendAsync("SendMessage", keypair, DateTime.Now.ToString("dd/MM/yyyy - H:mm"), Global_Variables.channelType);
                }
            }
            catch (Exception){}
        }
        public static async void Send_ImageMessage(string msg, string imageID)
        {
            try
            {
                var key = new AesManaged().Key;
                var vector = new AesManaged().IV;
                var crypted = CryptMessage.Encrypt(msg, key, vector);
                var keypair = new Dictionary<string, string>
                    {
                        {"key", Convert.ToBase64String(key) + "!" + Convert.ToBase64String(vector)},
                        {"message", Convert.ToBase64String(crypted)}
                    };
                await Connector.connection.SendAsync("Send_ImageMessage", imageID, keypair, DateTime.Now.ToString("dd/MM/yyyy - H:mm"), Global_Variables.channelType);
            }
            catch (Exception){ }
        }
    }
}
