using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.FacebookShop.Domains;

namespace NopStation.Plugin.Misc.FacebookShop.Services
{
    public partial interface IFacebookShopIOManager
    {
        Task WriteOrUpdateShopItemToExcelAsync();
    }
}
