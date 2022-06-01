 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.NopStation.FacebookShop.Domains;

namespace Nop.Plugin.NopStation.FacebookShop.Services
{
    public partial interface IFacebookShopIOManager
    {
        Task WriteOrUpdateShopItemToExcelAsync();
    }
}
