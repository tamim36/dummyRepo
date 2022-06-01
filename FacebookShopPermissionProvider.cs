using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace NopStation.Plugin.Misc.FacebookShop
{
    public class FacebookShopPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageFacebookShop = new PermissionRecord { Name = "NopStation FacebookShop. Manage facebookshop", SystemName = "ManageNopStationFacebookShop", Category = "NopStation" };

        public HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageFacebookShop
                    }
                )
            };
        }

        public IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageFacebookShop
            };
        }
    }
}