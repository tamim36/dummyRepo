using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.FacebookShop.Factories;
using NopStation.Plugin.Misc.FacebookShop.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.FacebookShop.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public int Order => 11;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.Misc.FacebookShop");

            services.AddScoped<IFacebookShopService, FacebookShopService>();
            services.AddScoped<IFacebookShopModelFactory, FacebookShopModelFactory>();
            services.AddScoped<IFacebookShopIOManager, FacebookShopIOManager>();
        }
    }
}