using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Infrastructure
{
    public class PluginNopStartup : INopStartup
    {
        public int Order => 11;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.FacebookShop");

            services.AddScoped<IShopItemModelFactory, ShopItemModelFactory>();
            services.AddScoped<IFacebookShopModelFactory, FacebookShopModelFactory>();
        }
    }
}