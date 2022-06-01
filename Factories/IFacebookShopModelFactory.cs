using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using NopStation.Plugin.Misc.FacebookShop.Domains;
using NopStation.Plugin.Misc.FacebookShop.Models;

namespace NopStation.Plugin.Misc.FacebookShop.Factories
{
    public partial interface IFacebookShopModelFactory
    {
        Task<Currency> GetPrimaryCurrencyAsync();

        Task<FacebookShopProductModel> PrepareFacebookShopProductModelAsync(Product product, ShopItem item);
    }
}