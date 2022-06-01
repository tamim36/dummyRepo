using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Models;
using NopStation.Plugin.Misc.FacebookShop.Domains;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Factories
{
    public partial interface IShopItemModelFactory
    {
        Task<ShopItemModel> PrepareShopItemModelAsync(ShopItemModel model, ShopItem shopItem, ProductModel productModel, bool excludeProperties = false);


    }
}
