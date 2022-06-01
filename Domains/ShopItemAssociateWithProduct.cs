using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.NopStation.FacebookShop.Domains
{
    public partial class ShopItemAssociateWithProduct
    {
        public Product Product { get; set; }
        public ShopItem ShopItem { get; set; }
    }
}
