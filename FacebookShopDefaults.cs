using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.NopStation.FacebookShop
{
    public static class FacebookShopDefaults
    {
        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string SystemName => "NopStation.FacebookShop";
        
        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string FileName => "catalog_products.csv";

        /// <summary>
        /// Gets the name of the view component to Facebook Shop Admin
        /// </summary>
        public const string FACEBOOK_SHOP_ADMIN_VIEW_COMPONENT_NAME = "FacebookShopAdmin";
        
        /// <summary>
        /// Gets the name of the view component to Product Items Bulk Upload
        /// </summary>
        public const string BULK_UPLOAD_SHOP_ITEMS_VIEW_COMPONENT_NAME = "ProductItemsBulkUpload";
    }
}
