using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.Core.Controllers;
using Nop.Plugin.NopStation.Core.Filters;
using Nop.Plugin.NopStation.Core.Helpers;
using Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Factories;
using Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Models;
using Nop.Plugin.NopStation.FacebookShop.Domains;
using Nop.Plugin.NopStation.FacebookShop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Controllers
{
    public class FacebookShopController : NopStationAdminController
    {
        #region Properties
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IFacebookShopModelFactory _facebookShopModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IShopItemModelFactory _shopItemModelFactory;
        private readonly IFacebookShopService _facebookShopService;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        #endregion

        #region Ctor
        public FacebookShopController(IPermissionService permissionService,
          ISettingService settingService,
          IFacebookShopModelFactory facebookShopModelFactory,
          IStoreContext storeContext,
          INotificationService notificationService,
          ILocalizationService localizationService,
          IShopItemModelFactory shopItemModelFactory, IFacebookShopService facebookShopService, IWorkContext workContext, ICategoryService categoryService, IProductService productService)
        {
            _permissionService = permissionService;
            _settingService = settingService;
            _facebookShopModelFactory = facebookShopModelFactory;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _shopItemModelFactory = shopItemModelFactory;
            _facebookShopService = facebookShopService;
            _workContext = workContext;
            _categoryService = categoryService;
            _productService = productService;
        }
        #endregion

        #region Action Methods
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(FacebookShopPermissionProvider.ManageFacebookShop))
                return AccessDeniedView();

            var model = await _facebookShopModelFactory.PrepareConfigurationModelAsync();
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FacebookShopPermissionProvider.ManageFacebookShop))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var facebookShopSettings = await _settingService.LoadSettingAsync<FacebookShopSettings>(storeScope);
            facebookShopSettings = model.ToSettings(facebookShopSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(facebookShopSettings, x => x.PrimaryCurrencyId, model.PrimaryCurrencyId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(facebookShopSettings, x => x.PrimaryLanguageId, model.PrimaryLanguageId_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> GetGoogleProductCategoriesList()
        {
            try
            {
                var nopFileProvider = NopInstance.Load<INopFileProvider>();
                var filePath = nopFileProvider.Combine(nopFileProvider.MapPath("~/Plugins/NopStation.FacebookShop/"), "GoogleProductCategories.json");

                if (nopFileProvider.FileExists(filePath))
                {
                    var jsonstr = await nopFileProvider.ReadAllTextAsync(filePath, Encoding.UTF8);
                    var list = JsonConvert.DeserializeObject(jsonstr);
                    return Json(list);
                }
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Json("");
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteSelectedProductShopItems(string selectedIds)
        {
            try
            {
                await _facebookShopService.BulkDeleteShopItemAsync(selectedIds);
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Json("");
        }

        public virtual async Task<IActionResult> BulkItemsAddPopup()
        {
            ViewBag.RefreshPage = false;
            var model = await _shopItemModelFactory.PrepareShopItemModelAsync(new ShopItemModel(), null, new ProductModel());
            return View("_BulkItemsAddPopup", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> BulkItemsAddPopup(string productIds, ShopItemModel model)
        {
            try
            {
                ViewBag.RefreshPage = true;
                await _facebookShopService.BulkInsertShopItemAsync(productIds, model.ToEntity<ShopItem>());
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.AddedSuccessfully"));
            }
            catch (Exception ex)
            {
                ViewBag.RefreshPage = false;

                return BadRequest(ex.Message);
            }
            return View("_BulkItemsAddPopup", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> BulkItemsAddAllView(ProductSearchModel productSearchModel)
        {
            ViewBag.RefreshPage = false;
            var shopItemModel = new ShopItemModel { ProductSearchModel = productSearchModel };

            var model = await _shopItemModelFactory.PrepareShopItemModelAsync(shopItemModel, null, new ProductModel());

            return View("_BulkItemsAddAllView", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> BulkItemsAddAll(ShopItemModel model)
        {
            try
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                    return AccessDeniedView();

                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() != null)
                {
                    model.ProductSearchModel.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;
                }

                var categoryIds = new List<int> { model.ProductSearchModel.SearchCategoryId };
                //include subcategories
                if (model.ProductSearchModel.SearchIncludeSubCategories && model.ProductSearchModel.SearchCategoryId > 0)
                    categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.ProductSearchModel.SearchCategoryId, showHidden: true));

                //0 - all (according to "ShowHidden" parameter)
                //1 - published only
                //2 - unpublished only
                bool? overridePublished = null;
                if (model.ProductSearchModel.SearchPublishedId == 1)
                    overridePublished = true;
                else if (model.ProductSearchModel.SearchPublishedId == 2)
                    overridePublished = false;

                var products = await _productService.SearchProductsAsync(0,
                    categoryIds: categoryIds,
                    manufacturerIds: new List<int> { model.ProductSearchModel.SearchManufacturerId },
                    storeId: model.ProductSearchModel.SearchStoreId,
                    vendorId: model.ProductSearchModel.SearchVendorId,
                    warehouseId: model.ProductSearchModel.SearchWarehouseId,
                    productType: model.ProductSearchModel.SearchProductTypeId > 0 ? (ProductType?)model.ProductSearchModel.SearchProductTypeId : null,
                    keywords: model.ProductSearchModel.SearchProductName,
                    showHidden: true,
                    overridePublished: overridePublished);

                var productIds = products.Select(item => item.Id).ToArray();
                await _facebookShopService.BulkInsertAllFoundShopItemsAsync(model.ToEntity<ShopItem>(), productIds);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.AddedSuccessfully"));
            }
            catch (Exception ex)
            {
                ViewBag.RefreshPage = false;

                return BadRequest(ex.Message);
            }

            ViewBag.RefreshPage = true;
            return RedirectToAction("List", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAllProductShopItems(ProductSearchModel model)
        {
            try
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                    return AccessDeniedView();

                //a vendor should have access only to his products
                if (await _workContext.GetCurrentVendorAsync() != null)
                {
                    model.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;
                }

                var categoryIds = new List<int> { model.SearchCategoryId };
                //include subcategories
                if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                    categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

                //0 - all (according to "ShowHidden" parameter)
                //1 - published only
                //2 - unpublished only
                bool? overridePublished = null;
                if (model.SearchPublishedId == 1)
                    overridePublished = true;
                else if (model.SearchPublishedId == 2)
                    overridePublished = false;

                var products = await _productService.SearchProductsAsync(0,
                    categoryIds: categoryIds,
                    manufacturerIds: new List<int> { model.SearchManufacturerId },
                    storeId: model.SearchStoreId,
                    vendorId: model.SearchVendorId,
                    warehouseId: model.SearchWarehouseId,
                    productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                    keywords: model.SearchProductName,
                    showHidden: true,
                    overridePublished: overridePublished);
                var productIds = products.Select(item => item.Id).ToArray();

                await _facebookShopService.BulkDeleteAllFoundShopItemAsync(productIds);
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.RemovedSuccessfully"));
            return RedirectToAction("List", "Product");

        }

        public async Task<IActionResult> UploadFacebookCatalogCsvFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFacebookCatalogCsvFile(CsvFileUpdateModel model)
        { 
            try
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                    return AccessDeniedView();
                if (model.CatalogFile== null)
                {
                    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.FileRequired"));
                    return View();
                }

                var fileExtension = Path.GetExtension(model.CatalogFile.FileName);
                if (string.IsNullOrEmpty(fileExtension) || !fileExtension.ToLower().Equals(".csv"))
                {
                    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.FileFormatNotSupported"));
                    return View();
                }

                await _facebookShopService.UploadFacebookCatalogCsvFileAsync(model.CatalogFile);
            }
            catch (Exception ex)
            {
                NopInstance.Load<ILogger>().ErrorAsync(ex.Message, ex).Wait();
                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.AnErrorOccurred"));
                return View();
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.FileUploadedSuccessfully"));
            return View();
        }
        #endregion
    }
}