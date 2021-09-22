using Halmid_Client.Connectors;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Halmid_Client.Windows.Loading
{
    class GetData_Server
    {
        public static async Task Receive_Data()
        {
            try
            {
                await Connector.connection.SendAsync("Get_UserData");
                await Connector.connection.SendAsync("Get_UserChannels");
                await Connector.connection.SendAsync("Get_UserFriends");
                await Connector.connection.SendAsync("Get_UserPrivates");
            }
            catch (Exception)
            {
                await Receive_Data();
            }
        }
    }
}
