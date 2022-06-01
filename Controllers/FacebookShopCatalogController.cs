using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.FacebookShop.Services;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Misc.FacebookShop.Controllers
{
    public class FacebookShopCatalogController : NopStationPublicController
    {
        #region Properties
        private readonly INopFileProvider _fileProvider;
        private readonly ILogger _logger;
        #endregion

        #region Ctor
        public FacebookShopCatalogController(INopFileProvider fileProvider,
            ILogger logger)
        {
            _fileProvider = fileProvider;
            _logger = logger;
        }
        #endregion

        #region Methods
        public async Task<IActionResult> GetCatalogFeed(string fileName)
        {
            try
            {
                var basePath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Misc.FacebookShop/Files/");
                var excelFilePath = _fileProvider.Combine(_fileProvider.MapPath(basePath), FacebookShopDefaults.FileName);
                var bytes = await _fileProvider.ReadAllBytesAsync(excelFilePath);
                return File(bytes, MimeTypes.TextXlsx, FacebookShopDefaults.FileName);
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync($"Failed to get feed: {exc.Message}", exc);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion
    }
}