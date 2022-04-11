using asp_net_po_schedule_management_server.DbConfig;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using asp_net_po_schedule_management_server.Services;
using asp_net_po_schedule_management_server.Services.ServicesImplementation;
using asp_net_po_schedule_management_server.Utils;
using Microsoft.EntityFrameworkCore;

namespace asp_net_po_schedule_management_server
{
    public sealed class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string jwtToken = Configuration.GetConnectionString("JwtKey");
            services.AddControllers();
            
            // strefa autentykacji i blokowania tras oraz odblokowywania przez JWT
            services.AddSingleton<IJwtAuthenticationManager>(new JwtAuthenticationManagerImplementation(jwtToken));
            JwtAuthenticationManagerImplementation.ImplementsJwtOnStartup(services, jwtToken);
            
            // strefa dodawania serwisów i ich implementacji
            services.AddScoped<IFirstEndpointService, FirstEndpointServiceImplementation>();
            
            // Dodawanie kontekstu bazy danych
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    Configuration.GetConnectionString("MySequelConnection"),
                    new MySqlServerVersion(GlobalConfigurer.DB_DRIVER)));
            
            // zmienia ścieżki routingu z wielkich liter na małe
            services.AddRouting(options => {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();

            // ustawianie polityki cors
            app.UseCors(options => options
                .SetIsOriginAllowed(url => env.IsDevelopment() 
                    ? url == $"http://localhost:{GlobalConfigurer.ANGULAR_PORT}"    // dla wersji developerskiej
                    : url == $"https://{GlobalConfigurer.ANGULAR_PROD}")            // dla wersji produkcyjnej
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );
            
            app.UseAuthorization();
            app.UseAuthentication();
            
            // umożliwia mapowanie endpointów na podstawie annotacji w kontrolerach
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}