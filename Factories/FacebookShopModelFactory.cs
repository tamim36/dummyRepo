using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Plugin.NopStation.FacebookShop.Domains;
using Nop.Plugin.NopStation.FacebookShop.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;

namespace Nop.Plugin.NopStation.FacebookShop.Factories
{
    public partial class FacebookShopModelFactory : IFacebookShopModelFactory
    {
        #region Propeties
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly FacebookShopSettings _facebookShopSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly ILanguageService _languageService;
        private readonly IProductService _productService;
        #endregion

        #region Ctor
        public FacebookShopModelFactory(ILocalizationService localizationService,
         IUrlRecordService urlRecordService,
         IPictureService pictureService,
         ICurrencyService currencyService,
         IPriceCalculationService priceCalculationService,
         ITaxService taxService,
         IStaticCacheManager staticCacheManager,
         IPriceFormatter priceFormatter,
         IWebHelper webHelper,
         IWorkContext workContext,
         IStoreContext storeContext,
         MediaSettings mediaSettings,
         OrderSettings orderSettings,
         CatalogSettings catalogSettings,
         FacebookShopSettings facebookShopSettings,
         CurrencySettings currencySettings,
         ILanguageService languageService,
         IProductService productService)
        {
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _currencyService = currencyService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _staticCacheManager = staticCacheManager;
            _priceFormatter = priceFormatter;
            _webHelper = webHelper;
            _workContext = workContext;
            _storeContext = storeContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _catalogSettings = catalogSettings;
            _facebookShopSettings = facebookShopSettings;
            _currencySettings = currencySettings;
            _languageService = languageService;
            _productService = productService;
        }
        #endregion

        #region Utilities
        public virtual async Task<Currency> GetPrimaryCurrencyAsync()
        {

            Currency primaryCurrency = null;
            var currency = await _currencyService.GetCurrencyByIdAsync(_facebookShopSettings.PrimaryCurrencyId);
            if (currency != null)
            {
                primaryCurrency = currency;
                return primaryCurrency;
            }

            var currencyFromSettings = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            if (currencyFromSettings != null)
            {
                primaryCurrency = currencyFromSettings;
                return primaryCurrency;
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            var defaultCurrency = (await _currencyService.GetAllCurrenciesAsync(storeId: store.Id)).FirstOrDefault();
            return defaultCurrency;
        }
        protected virtual async Task<Language> GetPrimaryLanguageAsync()
        {
            Language primaryLanguage = null;
            var language = await _languageService.GetLanguageByIdAsync(_facebookShopSettings.PrimaryLanguageId);
            if (language != null)
            {
                primaryLanguage = language;
                return primaryLanguage;
            }
            var store = await _storeContext.GetCurrentStoreAsync();
            var defaultLanguage = (await _languageService.GetAllLanguagesAsync(storeId: store.Id)).FirstOrDefault();
            return defaultLanguage;
        }
        protected virtual async Task<(PictureModel pictureModel, IList<PictureModel> allPictureModels)> PrepareProductDetailsPictureModelAsync(Product product, bool isAssociatedProduct)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //prepare picture models
            var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDetailsPicturesModelKey
                , product, defaultPictureSize, isAssociatedProduct,
                await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                var defaultPicture = pictures.FirstOrDefault();

                string fullSizeImageUrl, imageUrl, thumbImageUrl;
                (imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, !isAssociatedProduct);
                (fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, !isAssociatedProduct);

                var defaultPictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl
                };
                //"title" attribute
                defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
                    defaultPicture.TitleAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                //"alt" attribute
                defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
                    defaultPicture.AltAttribute :
                    string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                //all pictures
                var pictureModels = new List<PictureModel>();
                for (var i = 0; i < pictures.Count(); i++)
                {
                    var picture = pictures[i];

                    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, !isAssociatedProduct);
                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

                    var pictureModel = new PictureModel
                    {
                        ImageUrl = imageUrl,
                        ThumbImageUrl = thumbImageUrl,
                        FullSizeImageUrl = fullSizeImageUrl,
                        Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
                        AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
                    };
                    //"title" attribute
                    pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
                        picture.TitleAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
                    //"alt" attribute
                    pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
                        picture.AltAttribute :
                        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

                    pictureModels.Add(pictureModel);
                }

                return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            });

            var allPictureModels = cachedPictures.PictureModels;
            return (cachedPictures.DefaultPictureModel, allPictureModels);
        }
        protected virtual async Task<FacebookShopProductModel.ProductPriceModel> PrepareProductPriceModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new FacebookShopProductModel.ProductPriceModel
            {
                ProductId = product.Id
            };

            model.HidePrices = false;
            if (product.CustomerEntersPrice)
            {
                model.CustomerEntersPrice = true;
            }
            else
            {
                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                {
                    model.CallForPrice = true;
                }
                else
                {
                    var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                    var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync(), includeDiscounts: false)).finalPrice);
                    var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync())).finalPrice);

                    //TODO: Change working currency
                    var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, await GetPrimaryCurrencyAsync());
                    var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, await GetPrimaryCurrencyAsync());
                    var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, await GetPrimaryCurrencyAsync());

                    if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                        model.OldPrice = Convert.ToString(oldPrice);

                    model.Price = Convert.ToString(finalPriceWithoutDiscount);

                    if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                        model.PriceWithDiscount = Convert.ToString(finalPriceWithDiscount);

                    model.PriceValue = finalPriceWithDiscount;

                    //property for German market
                    //we display tax/shipping info only with "shipping enabled" for this product
                    //we also ensure this it's not free shipping
                    model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                        && product.IsShipEnabled &&
                        !product.IsFreeShipping;

                    //PAngV baseprice (used in Germany)
                    model.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase);
                    //currency code
                    model.CurrencyCode = (await GetPrimaryCurrencyAsync()).CurrencyCode;

                    //rental
                    if (product.IsRental)
                    {
                        model.IsRental = true;
                        var priceStr = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                        model.RentalPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                    }
                }
            }

            return model;
        }
        #endregion

        #region Methods
        public async Task<FacebookShopProductModel> PrepareFacebookShopProductModelAsync(Product product, ShopItem item)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //standard properties
            var model = new FacebookShopProductModel
            {
                ProductId = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, (await GetPrimaryLanguageAsync()).Id),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription, (await GetPrimaryLanguageAsync()).Id),
                FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription, (await GetPrimaryLanguageAsync()).Id),
                SeName = await _urlRecordService.GetSeNameAsync(product, (await GetPrimaryLanguageAsync()).Id),
                Sku = product.Sku,
                Brand = await _localizationService.GetLocalizedAsync(item, x => x.Brand, (await GetPrimaryLanguageAsync()).Id),
                GoogleCategory = item.GoogleProductCategory.Equals(0) ? string.Empty : Convert.ToString(item.GoogleProductCategory),
                StockAvailability = await _productService.FormatStockMessageAsync(product, string.Empty),
                Condition = item.ProductConditionType.ToString()
            };

            //Pictures
            IList<PictureModel> allPictureModels;
            (model.DefaultPictureModel, model.PictureModels) = await PrepareProductDetailsPictureModelAsync(product, false);

            //Price
            model.ProductPrice = await PrepareProductPriceModelAsync(product);
            return model;
        }
        #endregion
    }
}