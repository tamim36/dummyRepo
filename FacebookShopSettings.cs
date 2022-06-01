using Nop.Core.Configuration;

namespace NopStation.Plugin.Misc.FacebookShop
{
    public class FacebookShopSettings : ISettings
    {
        public int PrimaryLanguageId { get; set; }
        public int PrimaryCurrencyId { get; set; }
    }
}