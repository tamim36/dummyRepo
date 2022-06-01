using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.FacebookShop
{
    public class FacebookShopPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IPermissionService _permissionService;

        public bool HideInWidgetList => false;

        public FacebookShopPlugin(IWebHelper webHelper,
            ILocalizationService localizationService,
            INopStationCoreService nopStationCoreService,
            IScheduleTaskService scheduleTaskService,
            IPermissionService permissionService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _nopStationCoreService = nopStationCoreService;
            _scheduleTaskService = scheduleTaskService;
            _permissionService = permissionService;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/FacebookShop/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new FacebookShopPermissionProvider());

            var task = await _scheduleTaskService.GetTaskByTypeAsync("NopStation.Plugin.Misc.FacebookShop.Services.FacebookShopItemTask");
            if (task == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask()
                {
                    Enabled = true,
                    Name = "Facebook shop upload items",
                    Seconds = 3600,
                    Type = "NopStation.Plugin.Misc.FacebookShop.Services.FacebookShopItemTask",
                    StopOnError = false
                });
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new FacebookShopPermissionProvider());

            var task = await _scheduleTaskService.GetTaskByTypeAsync("NopStation.Plugin.Misc.FacebookShop.Services.FacebookShopItemTask");
            if (task != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(task);
            }

            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {

            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.Menu.FacebookShop")
            };

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.Menu.Configuration"),
                Url = $"{_webHelper.GetStoreLocation()}Admin/FacebookShop/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "FacebookShop.Configuration"
            };
            menu.ChildNodes.Add(configItem);

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/facebook-shop",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);

        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Menu.FacebookShop", "Facebook shop"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Tab.Enable", "Facebook shop"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.IncludeInFacebookShop", "Include in facebook shop"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.IncludeInFacebookShop.Hint", "Include in facebook shop"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.GenderTypeId", "Gender"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.GenderTypeId.Hint", "Gender"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.GoogleProductCategory", "Google product category"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.GoogleProductCategory.Hint", "Input the Google Product Category Id"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.GoogleProductCategory.Source", "See the Google Product Categories Id from here"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.IsOverwriteBrandSelected", "Override Brand Name"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.IsOverwriteBrandSelected.Hint", "Override manufacturer name(Default as Brand Name) with given brand name"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.Brand", "Brand"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.Brand.Hint", "Brand Name"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.AgeGroupType", "Age Group"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.AgeGroupType.Hint", "The age group that the item is targeted towards."),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryLanguageId", "Primary language"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryLanguageId.Hint", "Primary language for facebook shop"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryCurrencyId", "Primary currency"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryCurrencyId.Hint", "Primary currency for facebook shop"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.ConfigurationDetails", "To use the bulk upload feature, you'll need to upload the data feed file in Facebook Commerce Manager. Follow the following steps, <br> <b> Facebook Commerce Manger > Choose your Catalog > Data sources > Data feed > Scheduled feed > Click Next > Enter this File URL: <i> ' _fileUrl_ ' </i> > Click Next > Enter the Scheduled time to update your facebook catalog items > Click Next > Configure your currency > Click Upload </b>" ),

                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AddValueForSelectedProduct", "Insert the following inputs for selected products"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AddAll", "Include the Products(All Found)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AddSelected", "Include the Products(Selected)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.RemoveAll", "Exclude the Product(All Found)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.RemoveSelected", "Exclude the Product(Selected)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.NoProductSelected", "No Products Selected"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AddedSuccessfully", "Added Successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.RemovedSuccessfully", "Removed Successfully"),

            };
            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                AdminWidgetZones.ProductDetailsBlock,
                AdminWidgetZones.ProductListButtons,
            });
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return widgetZone.Equals(AdminWidgetZones.ProductListButtons)
                ? FacebookShopDefaults.BULK_UPLOAD_SHOP_ITEMS_VIEW_COMPONENT_NAME
                : FacebookShopDefaults.FACEBOOK_SHOP_ADMIN_VIEW_COMPONENT_NAME;
        }
    }
}