using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Models;
using NopStation.Plugin.Misc.FacebookShop.Domains;
using NopStation.Plugin.Misc.FacebookShop.Services;
using Nop.Services;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Factories;

namespace NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Factories
{
    public partial class ShopItemModelFactory : IShopItemModelFactory
    {
        #region Properties
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        #endregion

        #region Ctor
        public ShopItemModelFactory(ILocalizationService localisationService,
         ILocalizedModelFactory localizedModelFactory)
        {
            _localizationService = localisationService;
            _localizedModelFactory = localizedModelFactory;
        }
        #endregion

        #region Methods
        protected async Task PrepareGenderTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var availableDataSourceTypes = (await GenderType.Male.ToSelectListAsync(false)).ToList();
            foreach (var source in availableDataSourceTypes)
            {
                items.Add(source);
            }

            if (withSpecialDefaultItem)
                items.Insert(0, new SelectListItem()
                {
                    Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                    Value = "0"
                });
        }
        protected List<SelectListItem> AvailableAgeGroupsItems()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Selected = true, Text = string.Empty, Value = ""},
                new SelectListItem { Selected = false, Text = "Adult", Value = "adult"},
                new SelectListItem { Selected = false, Text = "All Ages", Value = "all ages"},
                new SelectListItem { Selected = false, Text = "Teen", Value = "teen"},
                new SelectListItem { Selected = false, Text = "Kids", Value = "kids"},
                new SelectListItem { Selected = false, Text = "Toddler", Value = "toddler"},
                new SelectListItem { Selected = false, Text = "Infant", Value = "infant"},
                new SelectListItem { Selected = false, Text = "New Born", Value = "newborn"},
            };
        }

        public async Task<ShopItemModel> PrepareShopItemModelAsync(ShopItemModel model, ShopItem shopItem, ProductModel productModel, bool excludeProperties = false)
        {
            Func<ShopItemLocalizedModel, int, Task> localizedModelConfiguration = null;


            if (shopItem != null)
            {
                if (model == null)
                {
                    model = shopItem.ToModel<ShopItemModel>();
                    model.ProductId = productModel.Id;
                }
                if (!excludeProperties)
                {
                    localizedModelConfiguration = async (locale, languageId) =>
                    {
                        locale.Brand = await _localizationService.GetLocalizedAsync(shopItem, entity => entity.Brand, languageId, false, false);
                        if (!string.IsNullOrEmpty(locale.Brand))
                            model.IsOverwriteBrandSelected = true;
                    };
                }
            }
            if (!excludeProperties)
            {
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);
                await PrepareGenderTypesAsync(model.AvailableGenders, false);
            }
            if (!string.IsNullOrEmpty(model.Brand))
                model.IsOverwriteBrandSelected = true;
            model.AvailableAgeGroups = AvailableAgeGroupsItems();

            return model;
        }
        #endregion
    }
}