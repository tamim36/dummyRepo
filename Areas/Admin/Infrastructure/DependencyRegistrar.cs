using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Factories;

namespace Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IShopItemModelFactory, ShopItemModelFactory>();
            services.AddScoped<IFacebookShopModelFactory, FacebookShopModelFactory>();
        }
    }
}