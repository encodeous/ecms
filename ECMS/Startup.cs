using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using ECMS.Data;
using ECMS.Service;
using ECMS.Security;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

namespace ECMS
{
    /// <summary>
    /// https://www.youtube.com/watch?v=BmAnSNfFGsc&list=PL4WEkbdagHIR0RBe_P4bai64UDqZEbQap&index=12
    /// auth tutorial
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();

            services.AddScoped<AuthenticationStateProvider, EcmsAuthenticationStateProvider>();
            services.AddSingleton<Authenticator>();
            services.AddSingleton<UserManagementService>();
            services.AddBlazoredLocalStorage(config => config.JsonSerializerOptions.WriteIndented = true);

            services.AddSingleton<HttpClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
            lifetime.ApplicationStopping.Register(OnShutdown);
            lifetime.ApplicationStarted.Register(OnStartup);
        }

        public void OnStartup()
        {
            var auth = (Authenticator) Program.EcmsHost.Services.GetService(typeof(Authenticator));
            auth.LoadData();
        }

        public void OnShutdown()
        {
            var auth = (Authenticator) Program.EcmsHost.Services.GetService(typeof(Authenticator));
            auth.SaveData();
        }
    }
}
