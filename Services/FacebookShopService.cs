using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Models;
using Nop.Plugin.NopStation.FacebookShop.Domains;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;

namespace Nop.Plugin.NopStation.FacebookShop.Services
{
    public partial class FacebookShopService : IFacebookShopService
    {
        #region Propertise
        private readonly IRepository<ShopItem> _shopItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly INopFileProvider _nopFileProvider;

        #endregion

        #region ctor
        public FacebookShopService(IRepository<ShopItem> shopItemRepository,
          IRepository<Product> productRepository,
          IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
          IWorkContext workContext,
          IAclService aclService,
          IStoreMappingService storeMappingService, INopFileProvider nopFileProvider)
        {
            _shopItemRepository = shopItemRepository;
            _productRepository = productRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _workContext = workContext;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _nopFileProvider = nopFileProvider;
        }
        #endregion

        #region Methods
        public async Task DeleteShopItemAsync(ShopItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await _shopItemRepository.DeleteAsync(item);
        }
        public async Task DeleteShopItemAsync(List<ShopItem> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await _shopItemRepository.DeleteAsync(item);
        }

        public async Task<ShopItem> GetShopItemByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _shopItemRepository.GetByIdAsync(id);
        }

        public async Task<ShopItem> GetShopItemByProductIdAsync(int productId)
        {
            if (productId == 0)
                return null;

            return await _shopItemRepository.Table.FirstOrDefaultAsync(x => x.ProductId == productId);
        }

        public async Task InsertShopItemAsync(ShopItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await _shopItemRepository.InsertAsync(item);
        }

        public async Task InsertShopItemAsync(List<ShopItem> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await _shopItemRepository.InsertAsync(item);
        }

        public async Task<IPagedList<ShopItemAssociateWithProduct>> GetShopItemAssociateWithProducts(int pageIndex = 0,
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
            bool? overridePublished = null)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            if (!showHidden)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.Id == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType) &&
                    (showHidden == false || LinqToDB.Sql.Between(DateTime.UtcNow, p.AvailableStartDateTimeUtc ?? DateTime.MinValue, p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)) &&
                    (priceMin == null || p.Price >= priceMin) &&
                    (priceMax == null || p.Price <= priceMax)
                select p;

            //join with Shop item
            var query = from p in productsQuery
                        join si in _shopItemRepository.Table on p.Id equals si.ProductId
                        select new ShopItemAssociateWithProduct
                        {
                            Product = p,
                            ShopItem = si
                        };
            return await query.ToPagedListAsync(pageIndex, pageSize);

        }

        public async Task<IPagedList<ShopItem>> SearchShopItemsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var shopItemQuery = _shopItemRepository.Table;

            return await shopItemQuery.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task UpdateShopItemAsync(ShopItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await _shopItemRepository.UpdateAsync(item);
        }

        public async Task BulkInsertShopItemAsync(string productIds, ShopItem shopItem)
        {
            var productItemsId = productIds != null ? productIds.Split(",").Select(int.Parse).ToArray() : Array.Empty<int>();
            var shopItemsToBeInserted = new List<ShopItem>();

            foreach (var productId in productItemsId)
            {
                var existingShopItem = await _shopItemRepository.Table.AnyAsync(item => item.ProductId.Equals(productId));
                if (existingShopItem)
                    continue;

                var newShopItem = new ShopItem
                {
                    ProductId = productId,
                    IncludeInFacebookShop = true,
                    GenderTypeId = shopItem.GenderTypeId,
                    GoogleProductCategory = shopItem.GoogleProductCategory,
                    Brand = shopItem.Brand,
                };
                shopItemsToBeInserted.Add(newShopItem);
            }

            await InsertShopItemAsync(shopItemsToBeInserted);
        }
        public async Task BulkInsertAllFoundShopItemsAsync(ShopItem shopItem, int[] productIdsToAdd)
        {
            var existingShopItemsProductIds = await (from shopItemTableItem in _shopItemRepository.Table
                                                         select shopItemTableItem.ProductId).ToArrayAsync();
            productIdsToAdd = productIdsToAdd.Except(existingShopItemsProductIds).ToArray();

            var shopItemsToBeInserted = productIdsToAdd.Select(productId => new ShopItem
            {
                ProductId = productId,
                IncludeInFacebookShop = true,
                GenderTypeId = shopItem.GenderTypeId,
                GoogleProductCategory = shopItem.GoogleProductCategory,
                Brand = shopItem.Brand,
            })
                .ToList();

            await InsertShopItemAsync(shopItemsToBeInserted);
        }

        public async Task BulkDeleteShopItemAsync(string productIds)
        {
            var productItemsId = productIds != null ? productIds.Split(",").Select(int.Parse).ToArray() : Array.Empty<int>();
            var shopItemsToBeDeleted = new List<ShopItem>();

            foreach (var productId in productItemsId)
            {
                var shopItem = await _shopItemRepository.Table.FirstOrDefaultAsync(item => item.ProductId.Equals(productId));
                if (shopItem != null)
                {
                    shopItemsToBeDeleted.Add(shopItem);
                }
            }
            await DeleteShopItemAsync(shopItemsToBeDeleted);
        }
        public async Task BulkDeleteAllFoundShopItemAsync(int[] productIdsToAdd)
        {
            var existingShopItemsProductIds = await (from shopItemTableItem in _shopItemRepository.Table
                                                         select shopItemTableItem.ProductId).ToArrayAsync();

            var shopItemsToBeDeleted = new List<ShopItem>();
            //existingShopItemsProductIds.Exist
            //Array.Exists(existingShopItemsProductIds, item=>item.c)

            var shopItemIdsToBeDeleted = existingShopItemsProductIds.Where(productIdsToAdd.Contains);
            foreach (var productId in shopItemIdsToBeDeleted)
            {
                var shopItem = await _shopItemRepository.Table.FirstAsync(item => item.ProductId.Equals(productId));

                shopItemsToBeDeleted.Add(shopItem);
            }
            await DeleteShopItemAsync(shopItemsToBeDeleted);
        }
        public async Task UploadFacebookCatalogCsvFileAsync(IFormFile catalogFile)
        {
            var basePath = _nopFileProvider.MapPath("~/Plugins/NopStation.FacebookShop/Files/");
            var excelFilePath = _nopFileProvider.Combine(_nopFileProvider.MapPath(basePath), FacebookShopDefaults.FileName);

            await catalogFile.CopyToAsync(new FileStream(excelFilePath, FileMode.Create));
        }
        #endregion
    }
}