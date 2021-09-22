using Halmid_Server.Database.Connectors;
using Halmid_Server.Functions;
using Halmid_Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Halmid_Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public static MySqlConnection connection;
        public static MySqlConnection connection2;

        public void ConfigureServices(IServiceCollection services)
        {
            connection = new MySqlConnection(Connection_Data.connectionString);
            connection2 = new MySqlConnection(Connection_Data.connectionString);
            connection.Open();
            connection2.Open();

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            services.AddSignalR();
            services.AddControllers();
            services.AddSingleton<IDictionary<string, User>>(e => new Dictionary<string, User>());
            services.AddHostedService<CheckUpdate>();
        }
        static async void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            await Task.Delay(2000);
            connection.Close();
            connection2.Close();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/ProtectedChannel");
            });
        }
    }
}
