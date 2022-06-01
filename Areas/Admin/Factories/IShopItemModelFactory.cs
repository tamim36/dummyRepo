using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Models;
using Nop.Plugin.NopStation.FacebookShop.Domains;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Factories
{
    public partial interface IShopItemModelFactory
    {
        Task<ShopItemModel> PrepareShopItemModelAsync(ShopItemModel model, ShopItem shopItem, ProductModel productModel, bool excludeProperties = false);

       
    }
}
