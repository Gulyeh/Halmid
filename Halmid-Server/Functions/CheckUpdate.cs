using Halmid_Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Halmid_Server.Functions
{
    public class CheckUpdate : BackgroundService
    {
        private IHubContext<ChatHub> HubContext { get; set; }
        public static string Version { get; set; }
        
        public CheckUpdate(IHubContext<ChatHub> hubContext)
        {
            HubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                int ifUpdate = 0;
                using (MySqlCommand cmd = Startup.connection2.CreateCommand())
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        cmd.CommandText = String.Format("SELECT Update_Client, Update_Version FROM update_allow");
                        using (MySqlDataReader readed = cmd.ExecuteReader())
                        {
                            while (readed.Read())
                            {
                                ifUpdate = Int32.Parse(readed.GetString("Update_Client"));
                                Version = readed.GetString("Update_Version");
                            }
                            readed.Close();
                        }

                        if (ifUpdate == 1)
                        {
                            cmd.CommandText = String.Format("UPDATE update_allow SET Update_Client = 0");
                            cmd.ExecuteScalar();
                            await HubContext.Clients.All.SendAsync("clientUpdate_Data", true);
                        }
                        await Task.Delay(10000, stoppingToken);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
