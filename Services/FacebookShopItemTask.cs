using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Services.ScheduleTasks;
using Task = System.Threading.Tasks.Task;

namespace NopStation.Plugin.Misc.FacebookShop.Services
{
    public class FacebookShopItemTask : IScheduleTask
    {
        #region Properties
        private readonly IFacebookShopIOManager _facebookShopIOManager;
        #endregion

        #region Ctor
        public FacebookShopItemTask(IFacebookShopIOManager facebookShopIOManager)
        {
            _facebookShopIOManager = facebookShopIOManager;
        }
        #endregion

        #region Methods
        public async Task ExecuteAsync()
        {
            await _facebookShopIOManager.WriteOrUpdateShopItemToExcelAsync();
        }
        #endregion
    }
}
