using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Models.Common;

namespace Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Factories
{
    public partial class FacebookShopModelFactory : IFacebookShopModelFactory
    {
        #region Properties
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService; 
        #endregion

        #region Ctor
        public FacebookShopModelFactory(IStoreContext storeContext,
          ISettingService settingService,
          ILanguageService languageService,
          ICurrencyService currencyService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _languageService = languageService;
            _currencyService = currencyService;
        } 
        #endregion

        #region Utilites
        private async Task<IList<SelectListItem>> GetLanguageSelectListAsync()
        {
            var availableLanguages = (await _languageService
                  .GetAllLanguagesAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id))
                  .Select(x => new LanguageModel
                  {
                      Id = x.Id,
                      Name = x.Name,
                      FlagImageFileName = x.FlagImageFileName,
                  });

            var list = new List<SelectListItem>();
            foreach (var aLanguage in availableLanguages)
            {
                list.Add(new SelectListItem()
                {
                    Value = aLanguage.Id.ToString(),
                    Text = aLanguage.Name
                });
            }
            return list;
        }
        private async Task<IList<SelectListItem>> GetCurrencySelectListAsync()
        {

            var availableCurrencies = (await _currencyService
                .GetAllCurrenciesAsync(storeId: (await _storeContext.GetCurrentStoreAsync()).Id))
                 .Select(aCurrency => new CurrencyModel
                 {
                     Id = aCurrency.Id,
                     Name = aCurrency.Name
                 });

            var list = new List<SelectListItem>();
            foreach (var aCurrency in availableCurrencies)
            {
                list.Add(new SelectListItem()
                {
                    Value = aCurrency.Id.ToString(),
                    Text = aCurrency.Name
                });
            }
            return list;
        }
        #endregion

        #region Methods
        public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var facebookShopSettings = await _settingService.LoadSettingAsync<FacebookShopSettings>(storeScope);

            var model = facebookShopSettings.ToSettingsModel<ConfigurationModel>();
            model.ActiveStoreScopeConfiguration = storeScope;

            model.AvailableLanguages = await GetLanguageSelectListAsync(); //BAseAdminModelFactory
            model.AvailableCurrencies = await GetCurrencySelectListAsync(); //BAseAdminModelFactory

            if (storeScope <= 0)
                return model;

            model.PrimaryCurrencyId_OverrideForStore = await _settingService.SettingExistsAsync(facebookShopSettings, x => x.PrimaryCurrencyId, storeScope);
            model.PrimaryLanguageId_OverrideForStore = await _settingService.SettingExistsAsync(facebookShopSettings, x => x.PrimaryLanguageId, storeScope);
            return model;


        } 
        #endregion
    }
}
