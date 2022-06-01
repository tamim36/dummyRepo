using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.NopStation.Core;
using Nop.Plugin.NopStation.Core.Helpers;
using Nop.Plugin.NopStation.Core.Services;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.NopStation.FacebookShop
{
    public class FacebookShopPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly Nop.Services.Tasks.IScheduleTaskService _scheduleTaskService;
        private readonly IPermissionService _permissionService;

        public bool HideInWidgetList => false;

        public FacebookShopPlugin(IWebHelper webHelper,
            ILocalizationService localizationService,
            INopStationCoreService nopStationCoreService,
            Nop.Services.Tasks.IScheduleTaskService scheduleTaskService,
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
            await this.NopStationPluginInstallAsync(new FacebookShopPermissionProvider());

            var task = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.NopStation.FacebookShop.Services.FacebookShopItemTask");
            if (task == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new Nop.Core.Domain.Tasks.ScheduleTask()
                {
                    Enabled = true,
                    Name = "Facebook shop upload items",
                    Seconds = 3600,
                    Type = "Nop.Plugin.NopStation.FacebookShop.Services.FacebookShopItemTask",
                    StopOnError = false
                });
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.NopStationPluginUninstallAsync(new FacebookShopPermissionProvider());

            var task = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.NopStation.FacebookShop.Services.FacebookShopItemTask");
            if (task != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(task);
            }

            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            if (await _permissionService.AuthorizeAsync(FacebookShopPermissionProvider.ManageFacebookShop))
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

                var uploadFileItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.FacebookShop.Menu.UploadFile"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/FacebookShop/UploadFacebookCatalogCsvFile",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "FacebookShop.Configuration"
                };
                menu.ChildNodes.Add(uploadFileItem);

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
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.CustomImageUrl", "Custom Image Url"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.CustomImageUrl.Hint", "Enter the Custom Image url to be appeared in Facebook shop.(It will also overwrite the default product image.)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.ProductCondition", "Product Condition"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.ProductCondition.Hint", "Enter the Products Condition. Following tag/ribbon will be on Facebook Shop Items.(Please be informed that, If the product have any discount than it will be overwritten with 'Sale' tag/ribbon)"),
                
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.ConfirmSubmit", "Are you sure you want to upload the following files?"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.CatalogFile", "Bulk Product Upload By CSV"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Fields.CatalogFile.Hint", "Upload the CSV file with your customization.You can download the present file to see the file template. Make sure you turn off the 'Facebook shop upload items' Schedule task to not update the uploaded file. For more details check here, 'https://www.facebook.com/business/help/120325381656392?id=725943027795860'. (WARNING! It will replace the existing file that's being generated automatically. Before upload, Download the existing file and take a back up of it )"),

                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryLanguageId", "Primary language"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryLanguageId.Hint", "Primary language for facebook shop"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryCurrencyId", "Primary currency"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration.Fields.PrimaryCurrencyId.Hint", "Primary currency for facebook shop"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.Menu.UploadFile", "File Upload"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.ConfigurationDetails", "To use the bulk upload feature, you'll need to upload the data feed file in Facebook Commerce Manager. Follow the following steps, <br> <b> Facebook Commerce Manger > Choose your Catalog > Data sources > Data feed > Scheduled feed > Click Next > Enter this File URL: <i> ' _fileUrl_ ' </i> > Click Next > Enter the Scheduled time to update your facebook catalog items > Click Next > Configure your currency > Click Upload </b>" ),

                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AddValueForSelectedProduct", "Insert the following inputs for selected products"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AddAll", "Include the Products(All Found)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AddSelected", "Include the Products(Selected)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.RemoveAll", "Exclude the Product(All Found)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.RemoveSelected", "Exclude the Product(Selected)"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.NoProductSelected", "No Products Selected"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AddedSuccessfully", "Added Successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.RemovedSuccessfully", "Removed Successfully"),

                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.FileRequired", "File Required"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.FileFormatNotSupported", "File Format is not Supported"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.FileUploadedSuccessfully", "File Uploaded Successfully"),
                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.AnErrorOccurred", "An Error Occurred!"),

                new KeyValuePair<string, string>("Admin.NopStation.FacebookShop.UploadYourCSVFile", "Upload your Product CSV File"),
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