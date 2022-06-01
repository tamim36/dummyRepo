using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Models;
using NopStation.Plugin.Misc.FacebookShop.Domains;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.FacebookShop.Services
{
    public class EventConsumer : IConsumer<ModelReceivedEvent<BaseNopModel>>
    {
        #region Propertise
        private readonly IProductService _productService;
        private readonly IFacebookShopService _facebookShopService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INopFileProvider _fileProvider;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        #endregion

        #region Ctor
        public EventConsumer(IProductService productService,
            IFacebookShopService facebookShopService,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _productService = productService;
            _facebookShopService = facebookShopService;
            _httpContextAccessor = httpContextAccessor;
            _fileProvider = fileProvider;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }
        #endregion

        #region Utilities
        private async Task UpdateLocalesAsync(ShopItem item, string brand)
        {
            var availableLanguages = await _languageService.GetAllLanguagesAsync(true);
            for (int i = 0; i < availableLanguages.Count; i++)
            {
                var brandLocalValue = $"Locales[{i}].Brand";
                var languageIdValue = $"Locales[{i}].LanguageId";
                if (CheckFormValue(brandLocalValue, out string returnedValue) && CheckFormValue(languageIdValue, out string languageId))
                {
                    await _localizedEntityService.SaveLocalizedValueAsync(item,
                           x => x.Brand, returnedValue, Convert.ToInt32(languageId));
                }
            }

        }
        private bool CheckFormValue(string key, out string returnedValue)
        {
            returnedValue = string.Empty;
            if (_httpContextAccessor.HttpContext.Request.Form.TryGetValue(key, out var value)
                && !StringValues.IsNullOrEmpty(value))
            {
                returnedValue = value.FirstOrDefault();
                return true;
            }
            return false;
        }
        private async Task CreateOrUpdateShopItemAsync(ShopItem item, bool isNew = false)
        {
            if (CheckFormValue("IncludeInFacebookShop", out var returnedValue))
                item.IncludeInFacebookShop = Convert.ToBoolean(returnedValue);
            if (CheckFormValue("GenderTypeId", out var genderId))
                item.GenderTypeId = Convert.ToInt32(genderId);
            if (CheckFormValue("GoogleProductCategory", out var googleProductCategory))
                item.GoogleProductCategory = Convert.ToInt32(googleProductCategory);
            if (CheckFormValue("Brand", out var brand))
                item.Brand = brand;
            if (CheckFormValue("AgeGroupType", out var ageGroupType))
                item.AgeGroupType = ageGroupType;
            if (isNew)
            {
                await _facebookShopService.InsertShopItemAsync(item);
            }
            else
            {
                await _facebookShopService.UpdateShopItemAsync(item);
            }

            //Update locales
            await UpdateLocalesAsync(item, item.Brand);
        }
        #endregion

        #region Methods
        public async Task HandleEventAsync(ModelReceivedEvent<BaseNopModel> eventMessage)
        {
            //get entity by received model
            var entity = eventMessage.Model switch
            {
                ProductModel productModel => await _productService.GetProductByIdAsync(productModel.Id),
                _ => null
            };
            if (entity == null)
                return;

            var isShopItemExists = await _facebookShopService.GetShopItemByProductIdAsync(entity.Id);
            if (isShopItemExists == null)
            {
                var aShopItem = new ShopItem { ProductId = entity.Id };
                await CreateOrUpdateShopItemAsync(aShopItem, true);
            }
            else
            {
                await CreateOrUpdateShopItemAsync(isShopItemExists);
            }
        }
        #endregion
    }
}