using Microsoft.AspNetCore.SignalR.Client;

namespace Halmid_Client.Connectors
{
    class Create_HubConnection
    {
        public static void NewConnection()
        {
            Connector.connection = new HubConnectionBuilder()
                        .WithUrl("http://31.178.21.16:7342/ProtectedChannel").Build();
        }
    }
}
