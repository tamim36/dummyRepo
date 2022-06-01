using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using AutoMapper.QueryableExtensions.Impl;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.NopStation.FacebookShop.Domains;
using Nop.Plugin.NopStation.FacebookShop.Factories;
using Nop.Plugin.NopStation.FacebookShop.Models;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NUglify.Helpers;
using IProductModelFactory = Nop.Web.Areas.Admin.Factories.IProductModelFactory;

namespace Nop.Plugin.NopStation.FacebookShop.Services
{
    public partial class FacebookShopIOManager : IFacebookShopIOManager
    {
        #region Properties

        private readonly IFacebookShopService _facebookShopService;
        private readonly ILogger _logger;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IFacebookShopModelFactory _facebookShopModelFactory;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPermissionService _permissionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWorkContext _workContext;
        private readonly Nop.Web.Areas.Admin.Factories.IProductModelFactory _productModelFactory;
        private readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly IPriceCalculationService _priceCalculationService;




        private string _currencyCode = string.Empty;

        #endregion

        #region Ctor
        public FacebookShopIOManager(IFacebookShopService facebookShopService,
            ILogger logger,
            INopFileProvider nopFileProvider,
            IFacebookShopModelFactory facebookShopModelFactory,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IStoreService storeService,
            IStoreContext storeContext, IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IShoppingCartService shoppingCartService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IWorkContext workContext,
            IProductModelFactory productModelFactory,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IPictureService pictureService,
            IProductAttributeParser productAttributeParser,
            IProductService productService, IPriceCalculationService priceCalculationService)
        {
            _facebookShopService = facebookShopService;
            _logger = logger;
            _nopFileProvider = nopFileProvider;
            _facebookShopModelFactory = facebookShopModelFactory;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _storeService = storeService;
            _storeContext = storeContext;
            _manufacturerService = manufacturerService;
            _permissionService = permissionService;
            _shoppingCartService = shoppingCartService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _workContext = workContext;
            _productModelFactory = productModelFactory;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _pictureService = pictureService;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _priceCalculationService = priceCalculationService;
        }

        #endregion

        #region Methods
        protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null,
            object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync()
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate a URL with an absolute path
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));

            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            //compose the result
            return Uri.EscapeUriString(WebUtility.UrlDecode($"{store.Url.TrimEnd('/')}{url}"));
        }

        public async Task WriteOrUpdateShopItemToExcelAsync()
        {
            var pageIndex = 0;
            var currentStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;

            try
            {
                var basePath = _nopFileProvider.MapPath("~/Plugins/NopStation.FacebookShop/Files/");
                var excelFilePath =
                    _nopFileProvider.Combine(_nopFileProvider.MapPath(basePath), FacebookShopDefaults.FileName);
                //var sw = await _nopFileProvider.ReadAllTextAsync(excelFilePath, Encoding.UTF8);
                //var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

                var records = new List<ItemModel>();

                using (var reader = new StreamReader(excelFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.CurrentCulture))
                {
                    records = csv.GetRecords<ItemModel>().ToList();
                }
                records.Clear();

                while (true)
                {
                    var shopItems = await _facebookShopService.GetShopItemAssociateWithProducts(pageIndex: pageIndex,
                        pageSize: 100, storeId: currentStoreId, showHidden: true);
                    if (shopItems == null || shopItems.Count == 0)
                        break;

                    foreach (var item in shopItems)
                    {
                        try
                        {
                            if (!item.ShopItem.IncludeInFacebookShop)
                                continue;

                            var model = await _facebookShopModelFactory.PrepareFacebookShopProductModelAsync(item.Product, item.ShopItem);

                            var aShopItem = new ItemModel();
                            await PrepareItemModelAsync(aShopItem, model, item);

                            var attributesWithValues = await GetItemModelsProductAttributeCombinationsWithValuesAsync(item.Product, aShopItem, model);
                            if (attributesWithValues.Count == 0) //if no attribute combination found insert the default prepared item model
                            {
                                records.Add(aShopItem);
                            }
                            else
                            {
                                records.AddRange(attributesWithValues);
                            }
                        }
                        catch (Exception ex)
                        {
                            await _logger.ErrorAsync(
                                "Facebook shop: " + ex.Message + ", Product Id = " + item.Product.Id +
                                ", Shop item Id = " + item.ShopItem.Id, ex);
                            continue;
                        }
                    }

                    pageIndex++;
                }

                //Write to csv file
                await using (var writer = new StreamWriter(excelFilePath, false, Encoding.UTF8))
                await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(records);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Facebook shop: " + ex.Message, ex);
                return;
            }
        }
        #endregion

        #region Utilities
        private async Task PrepareItemModelAsync(ItemModel itemModel, FacebookShopProductModel model, ShopItemAssociateWithProduct item)
        {
            _currencyCode = model.ProductPrice.CurrencyCode;

            var currentStoreId = (await _storeContext.GetCurrentStoreAsync()).Id;

            itemModel.Id = Convert.ToString(model.ProductId);
            itemModel.Title = model.Name;

            itemModel.Description = string.IsNullOrEmpty(model.ShortDescription)
                ? model.FullDescription
                : model.ShortDescription;

            if (!string.IsNullOrEmpty(model.FullDescription))
            {
                itemModel.Rich_Text_Description = Regex.Replace(model.FullDescription, @"[^\u0000-\u007F]+", string.Empty);
            }
            itemModel.Link = await RouteUrlAsync(currentStoreId, "Product", new { SeName = model.SeName });

            itemModel.Price = await FormatPriceForFacebookAsync(string.IsNullOrEmpty(model.ProductPrice.OldPrice)
                ? model.ProductPrice.Price
                : model.ProductPrice.OldPrice);
            itemModel.Sale_Price = await FormatPriceForFacebookAsync(string.IsNullOrEmpty(model.ProductPrice.PriceWithDiscount)
                ? model.ProductPrice.Price
                : model.ProductPrice.PriceWithDiscount);

            itemModel.Image_Link = string.IsNullOrEmpty(item.ShopItem.CustomImageUrl) 
                ? model.DefaultPictureModel.FullSizeImageUrl 
                : item.ShopItem.CustomImageUrl;

            itemModel.Additional_Image_Link = model.PictureModels?.Any() ?? false
                ? model.PictureModels.Last().FullSizeImageUrl
                : "";

            //itemModel.Delete = Convert.ToString(!item.ShopItem.IncludeInFacebookShop).ToLower();
            if (item.Product.Published && !item.Product.Deleted && item.ShopItem.IncludeInFacebookShop)
            {
                itemModel.Delete = "false";
            }
            else
            {
                itemModel.Delete = "true";
            }
            itemModel.Brand = string.IsNullOrEmpty(model.Brand) ? await GetProductManufacturerNameAsync(item.Product.Id) : model.Brand;
            if (!string.IsNullOrEmpty(model.StockAvailability))
            {
                itemModel.Availability = Regex.Replace(model.StockAvailability, @"[\d-]", string.Empty);
            }
            itemModel.Condition = model.Condition;
            itemModel.Google_Product_Category = model.GoogleCategory;
            itemModel.Gender = Convert.ToString(item.ShopItem.GenderType);
            itemModel.Item_Group_Id = Convert.ToString(model.ProductId);

            if (string.IsNullOrEmpty(itemModel.Availability))
            {
                itemModel.Availability = "In Stock"; // default value 
            }

            itemModel.Age_Group = item.ShopItem.AgeGroupType;
        }
        private async Task<List<ItemModel>> GetItemModelsProductAttributeCombinationsWithValuesAsync(Product product, ItemModel itemModel, FacebookShopProductModel rootProductModel)
        {
            #region Getting all Attribute Combition Name with thier values
            var prepareProductAttributeCombinationListModel = await _productModelFactory
                .PrepareProductAttributeCombinationListModelAsync(new ProductAttributeCombinationSearchModel { ProductId = product.Id, Length = int.MaxValue }, product);//TODO: Get only the facebook attribute combination
            var itemModels = new List<ItemModel>();
            #endregion
            if (prepareProductAttributeCombinationListModel.Data?.Any() ?? false)
            {
                foreach (var item in prepareProductAttributeCombinationListModel.Data)
                {
                    var attributeCombination = await _productAttributeCombinationRepository.GetByIdAsync(item.Id);
                    var itemModelCopy = new ItemModel
                    {
                        Id = $"{Convert.ToString(product.Id)}_{attributeCombination.Id}",
                        Title = itemModel.Title,
                        Description = itemModel.Description,
                        Rich_Text_Description = !string.IsNullOrEmpty(itemModel.Rich_Text_Description) ?
                            Regex.Replace(itemModel.Rich_Text_Description, @"[^\u0000-\u007F]+", string.Empty)
                            : "",
                        Link = itemModel.Link,
                        Price = itemModel.Price,
                        Sale_Price = await FormatPriceForFacebookAsync(await GetProductAttributeCombinationSalePriceAsync(product, attributeCombination.AttributesXml)),
                        //Image_Link = !string.IsNullOrEmpty(item.PictureThumbnailUrl) ? item.PictureThumbnailUrl : itemModel.Image_Link,
                        Image_Link = await GetProductAttributePictureUrlAsync(attributeCombination.AttributesXml, item.PictureId) ?? itemModel.Image_Link,
                        //Additional_Image_Link = await GetProductAttributePictureUrlAsync(product, item.AttributesXml) ?? itemModel.Additional_Image_Link,
                        Delete = itemModel.Delete,
                        Brand = itemModel.Brand,
                        Availability = Regex.Replace(await _productService.FormatStockMessageAsync(product, attributeCombination.AttributesXml), @"[\d-]", string.Empty),
                        Condition = itemModel.Condition,
                        Google_Product_Category = itemModel.Google_Product_Category,
                        Gender = itemModel.Gender,
                        Item_Group_Id = Convert.ToString(item.ProductId),
                        Age_Group = itemModel.Age_Group
                    };
                    if (string.IsNullOrEmpty(itemModelCopy.Availability))
                    {
                        itemModelCopy.Availability = "In Stock"; // default value 
                    }
                    //var salePriceDecimal = decimal.TryParse(await GetProductAttributeCombinationSalePriceAsync(product, attributeCombination.AttributesXml), out var price) ? price : 0;

                    //var priceDecimal = decimal.TryParse(rootProductModel.ProductPrice.Price, out var decimalValue) ? decimalValue : 0;

                    var salePriceDecimal = decimal.TryParse(!string.IsNullOrEmpty(itemModelCopy.Sale_Price)
                        ? Regex.Replace(itemModelCopy.Sale_Price, @"[^0-9.]+", "")
                        : "", out var decimalPrice) ? decimalPrice : 0;
                    var priceDecimal = decimal.TryParse(Regex.Replace(itemModelCopy.Price, @"[^0-9.]+", ""), out var decimalValue) ? decimalValue : 0;
                    if (salePriceDecimal > priceDecimal)
                    {
                        itemModelCopy.Price = await FormatPriceForFacebookAsync(Convert.ToString(salePriceDecimal));
                        itemModelCopy.Sale_Price = "";
                    }

                    var attributes = item.AttributesXml.Split("<br />");
                    foreach (var attribute in attributes)
                    {
                        var nameAndValue = attribute.Split(":");
                        if (!string.IsNullOrEmpty(nameAndValue.Last()))
                        {
                            if (nameAndValue.First().Equals("Color", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var theValue = Regex.Replace(nameAndValue.Last(), "\\[[^\\]]*\\]", "");
                                theValue = HttpUtility.HtmlDecode(theValue);
                                theValue = Regex.Replace(theValue, @"[^0-9a-zA-Z]+", "");

                                itemModelCopy.Color = theValue;
                                itemModels.Add(itemModelCopy);
                            }
                            if (nameAndValue.First().Equals("Material", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var theValue = Regex.Replace(nameAndValue.Last(), "\\[[^\\]]*\\]", "");
                                theValue = HttpUtility.HtmlDecode(theValue);
                                theValue = Regex.Replace(theValue, @"[^0-9a-zA-Z]+", "");

                                var index = itemModels.FindIndex(c =>
                                    c.Id == item.Id.ToString()); // whether the item model added previously
                                if (index != -1)
                                {
                                    itemModels[index].Material = theValue; // update the item model
                                }
                                else
                                {
                                    itemModelCopy.Material = theValue;
                                    itemModels.Add(itemModelCopy);
                                }
                            }
                            if (nameAndValue.First().Equals("Size", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var theValue = Regex.Replace(nameAndValue.Last(), "\\[[^\\]]*\\]", "");
                                theValue = HttpUtility.HtmlDecode(theValue);
                                theValue = Regex.Replace(theValue, @"[^0-9a-zA-Z]+", "");

                                var index = itemModels.FindIndex(c =>
                                    c.Id == item.Id.ToString()); // whether the item model added previously
                                if (index != -1)
                                {
                                    itemModels[index].Size = theValue; // update the item model
                                }
                                else
                                {
                                    itemModelCopy.Size = theValue;
                                    itemModels.Add(itemModelCopy);
                                }
                            }
                        }
                    }
                }
            }

            itemModels = itemModels.DistinctBy(x => x.Id).ToList();
            itemModels = itemModels.GroupBy(m => new { m.Sale_Price, m.Color, m.Size })
                .Select(group => group.Last())  // instead of First you can also apply your logic here what you want to take, for example an OrderBy
                .ToList();
            return itemModels;
        }

        private async Task<string> GetProductManufacturerNameAsync(int productId)
        {
            var productManufacturer = (await _manufacturerService.GetProductManufacturersByProductIdAsync(productId))
                .FirstOrDefault();
            if (productManufacturer != null)
            {
                var manufacture =
                    await _manufacturerService.GetManufacturerByIdAsync(productManufacturer.ManufacturerId);
                if (manufacture != null)
                {
                    return manufacture.Name;
                }
            }

            return null;
        }
        private async Task<string> GetProductAttributeCombinationSalePriceAsync(Product product, string attributeXml)
        {

            //we do not calculate price of "customer enters price" option is enabled
            var (finalPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(product,
                await _workContext.GetCurrentCustomerAsync(),
                ShoppingCartType.ShoppingCart,
                1, attributeXml, 0,
                null, null, true);
            var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, finalPrice);
            var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await _workContext.GetWorkingCurrencyAsync());
            // discounted price
            return finalPriceWithDiscount != product.Price ?
                Convert.ToString(finalPriceWithDiscount, CultureInfo.InvariantCulture) :
                Convert.ToString(product.Price, CultureInfo.InvariantCulture);
        }

        private async Task<string> FormatPriceForFacebookAsync(string price)
        {
            if (string.IsNullOrEmpty(price))
                return string.Empty;

            //if (price.Contains("$"))
            //{
            //    price = price?.Replace("$", "");
            //}
            var converted = decimal.TryParse(price, out var decimalPrice);
            return converted ? $"{await _priceCalculationService.RoundPriceAsync(decimalPrice)} { _currencyCode }" : "";
        }

        private async Task<string> GetProductAttributePictureUrlAsync(string attributeXml, int combinationsPictureId) //TODO: Start from here
        {
            string pictureUrl;

            #region CombinationPrictureId
            if (combinationsPictureId != 0)
            {
                pictureUrl = await GetPictureUrlByAttributeCombinationPictureIdAsync(combinationsPictureId);
                if (!string.IsNullOrEmpty(pictureUrl))
                    return pictureUrl;
            }
            #endregion

            #region Attributexml
            pictureUrl = await GetPictureUrlByAttributeXmlAsync(attributeXml);
            if (!string.IsNullOrEmpty(pictureUrl))
                return pictureUrl;
            #endregion

            return null;
        }

        private async Task<string> GetPictureUrlByAttributeXmlAsync(string attributeXml)
        {
            //then, let's see whether we have attribute values with pictures
            var attributePicture = await (await _productAttributeParser.ParseProductAttributeValuesAsync(attributeXml))
                .SelectAwait(async attributeValue => await _pictureService.GetPictureByIdAsync(attributeValue?.PictureId ?? 0))
                .FirstOrDefaultAsync(picture => picture != null);

            if (attributePicture == null)
                return null;

            var res = await _pictureService.GetPictureUrlAsync(attributePicture, showDefaultPicture: false);
            return res.Url;

        }

        private async Task<string> GetPictureUrlByAttributeCombinationPictureIdAsync(int pictureId)
        {
            var picture = await _pictureService.GetPictureByIdAsync(pictureId);
            var res = await _pictureService.GetPictureUrlAsync(picture, showDefaultPicture: false);
            return res.Url;
        }
        #endregion
    }
}
