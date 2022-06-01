using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.NopStation.FacebookShop.Factories;
using Nop.Plugin.NopStation.FacebookShop.Services;

namespace Nop.Plugin.NopStation.FacebookShop.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 11;

        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IFacebookShopService, FacebookShopService>();
            services.AddScoped<IFacebookShopModelFactory, FacebookShopModelFactory>();
            services.AddScoped<IFacebookShopIOManager, FacebookShopIOManager>();
        }
    }
}