using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Models
{
    public record ShopItemModel : BaseNopEntityModel, ILocalizedModel<ShopItemLocalizedModel>
    {
        public ShopItemModel()
        {
            ProductSearchModel = new ProductSearchModel();
            AvailableGenders = new List<SelectListItem>();
            AvailableAgeGroups = new List<SelectListItem>();
            AvailableProductConditions = new List<SelectListItem>();
            Locales = new List<ShopItemLocalizedModel>();
        }
        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.IncludeInFacebookShop")]
        public bool IncludeInFacebookShop { get; set; }

        public int ProductId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.GenderTypeId")]
        public int GenderTypeId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.GoogleProductCategory")]
        public int GoogleProductCategory { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.FacebookProductCategory")]
        public int FacebookProductCategory { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.IsOverwriteBrandSelected")]
        public bool IsOverwriteBrandSelected { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.Brand")]
        public string Brand { get; set; }
        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.AgeGroupType")]
        public string AgeGroupType { get; set; }
        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.CustomImageUrl")]
        public string CustomImageUrl { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.ProductCondition")]
        public int ProductCondition { get; set; }

        public ProductSearchModel ProductSearchModel { get; set; }

        public IList<SelectListItem> AvailableGenders { get; set; }
        public IList<SelectListItem> AvailableAgeGroups { get; set; }
        public IList<SelectListItem> AvailableProductConditions { get; set; }
        public IList<ShopItemLocalizedModel> Locales { get; set; }
    }
    public class ShopItemLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.Brand")]
        public string Brand { get; set; }
    }
}