using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Plugin.NopStation.FacebookShop.Domains;
using Nop.Plugin.NopStation.FacebookShop.Models;

namespace Nop.Plugin.NopStation.FacebookShop.Factories
{
    public partial interface IFacebookShopModelFactory
    {
        Task<Currency> GetPrimaryCurrencyAsync();

        Task<FacebookShopProductModel> PrepareFacebookShopProductModelAsync(Product product, ShopItem item);
    }
}