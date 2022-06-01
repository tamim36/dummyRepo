using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Factories;
using NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Models;
using NopStation.Plugin.Misc.FacebookShop.Services;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Components
{
    [ViewComponent(Name = FacebookShopDefaults.FACEBOOK_SHOP_ADMIN_VIEW_COMPONENT_NAME)]
    public class FacebookShopAdminViewComponent : NopStationViewComponent
    {
        #region Properties
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IProductService _productService;
        private readonly IShopItemModelFactory _shopItemModelFactory;
        private readonly IFacebookShopService _facebookShopService;
        #endregion

        #region Ctor
        public FacebookShopAdminViewComponent(IWidgetPluginManager widgetPluginManager,
         IProductService productService,
         IShopItemModelFactory shopItemModelFactory,
         IFacebookShopService facebookShopService)
        {
            _widgetPluginManager = widgetPluginManager;
            _productService = productService;
            _shopItemModelFactory = shopItemModelFactory;
            _facebookShopService = facebookShopService;
        }
        #endregion

        #region Properties
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(ProductModel))
                return Content("");

            if (!await _widgetPluginManager.IsPluginActiveAsync(FacebookShopDefaults.SystemName))
                return Content(string.Empty);

            var productModel = additionalData as ProductModel;

            var product = await _productService.GetProductByIdAsync(productModel.Id);
            if (product == null || product.Deleted)
                return Content("");

            var existingShopItem = await _facebookShopService.GetShopItemByProductIdAsync(productModel.Id);

            var model = await _shopItemModelFactory.PrepareShopItemModelAsync((existingShopItem == null ? new ShopItemModel() : null), (existingShopItem == null ? null : existingShopItem), productModel);
            return View(model);
        }
        #endregion
    }
}
