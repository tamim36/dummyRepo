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
    [ViewComponent(Name = FacebookShopDefaults.BULK_UPLOAD_SHOP_ITEMS_VIEW_COMPONENT_NAME)]
    public class ProductItemsBulkUploadViewComponent : NopStationViewComponent
    {
        #region Properties
        private readonly IWidgetPluginManager _widgetPluginManager;
        #endregion

        #region Ctor
        public ProductItemsBulkUploadViewComponent(IWidgetPluginManager widgetPluginManager)
        {
            _widgetPluginManager = widgetPluginManager;
        }
        #endregion

        #region Properties
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _widgetPluginManager.IsPluginActiveAsync(FacebookShopDefaults.SystemName)) //TODO: get rid of magic string
                return Content(string.Empty);

            return View();
        }
        #endregion
    }
}
