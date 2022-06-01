using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.NopStation.FacebookShop.Domains;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.NopStation.FacebookShop.Services
{
    public partial interface IFacebookShopService
    {
        Task<ShopItem> GetShopItemByIdAsync(int id);

        Task<ShopItem> GetShopItemByProductIdAsync(int id);

        Task InsertShopItemAsync(ShopItem item);
        Task InsertShopItemAsync(List<ShopItem> item);

        Task UpdateShopItemAsync(ShopItem item);

        Task DeleteShopItemAsync(ShopItem item);
        Task DeleteShopItemAsync(List<ShopItem> item);


        Task<IPagedList<ShopItem>> SearchShopItemsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        
        Task<IPagedList<ShopItemAssociateWithProduct>> GetShopItemAssociateWithProducts(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null);

        Task BulkInsertShopItemAsync(string productIds, ShopItem shopItem);
        Task BulkInsertAllFoundShopItemsAsync(ShopItem shopItem, int[] productIdsToAdd);

        Task BulkDeleteShopItemAsync(string productIds);
        Task BulkDeleteAllFoundShopItemAsync(int[] productIdsToAdd);

        Task UploadFacebookCatalogCsvFileAsync(IFormFile catalogFile);

    }
} 