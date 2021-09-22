using System.Threading.Tasks;

namespace Halmid_Client.Connectors
{
    class Connect_Server
    {
        public static async Task Connect()
        {
            await Connector.connection.StartAsync();
        }
    }
}
