using Nop.Core;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.NopStation.FacebookShop.Domains
{
    public partial class ShopItem : BaseEntity, ILocalizedEntity
    {
        public int ProductId { get; set; }
        public bool IncludeInFacebookShop { get; set; }
        public int GenderTypeId { get; set; }
        public int GoogleProductCategory { get; set; }
        public int FacebookProductCategory { get; set; }
        public string Brand { get; set; }
        public string AgeGroupType { get; set; }
        public string CustomImageUrl { get; set; } 
        public int ProductCondition { get; set; } 

        public GenderType GenderType 
        {
            get => (GenderType)GenderTypeId;
            set => GenderTypeId = (int)value;
        }

        public ProductConditionType ProductConditionType
        {
            get => (ProductConditionType)ProductCondition;
            set => ProductCondition = (int)value;
        }
    }
}