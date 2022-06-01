using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Models
{
    public class CsvFileUpdateModel
    {
        [NopResourceDisplayName("Admin.NopStation.FacebookShop.Fields.CatalogFile")]
        public IFormFile CatalogFile { get; set; }
    }
}
