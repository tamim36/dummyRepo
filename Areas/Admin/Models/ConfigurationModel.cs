using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableLanguages = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryLanguageId")]
        public int PrimaryLanguageId { get; set; }
        public bool PrimaryLanguageId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableLanguages { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryCurrencyId")]
        public int PrimaryCurrencyId { get; set; }
        public bool PrimaryCurrencyId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCurrencies { get; set; }
        public int ActiveStoreScopeConfiguration { get; set; }
    }
}