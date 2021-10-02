using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Halmid_WebApi
{
    public class Startup
    {
        private static readonly string SALT = "b697fb300b7f9f6826f0aceace0f5480scb697fb300b7f9f6826f0aceace0f5480=b697fb300b7f9f6826f0aceace0f5480scb697fb300b7f9f6826f0aceace0f5480=b697fb300b7f9f6826f0aceace0f5480scb697fb300b7f9f6826f0aceace0f5480=b697fb300b7f9f6826f0aceace0f5480scb697fb300b7f9f6826f0aceace0f5480=";
        private static readonly SymmetricSecurityKey TOKEN_KEY = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SALT));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer("Bearer", jwtopts =>
            {
                jwtopts.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = TOKEN_KEY,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
