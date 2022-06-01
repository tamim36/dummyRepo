using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Models;

namespace Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Factories
{
    public partial interface IFacebookShopModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();
    }
}
