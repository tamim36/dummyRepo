using Nop.Core.Configuration;

namespace Nop.Plugin.NopStation.FacebookShop
{
    public class FacebookShopSettings : ISettings
    {
        public int PrimaryLanguageId { get; set; }
        public int PrimaryCurrencyId { get; set; }
    }
}