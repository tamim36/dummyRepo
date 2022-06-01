using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.NopStation.FacebookShop.Domains;

namespace Nop.Plugin.NopStation.FacebookShop.Data
{
    public class ShopItemRecordBuilder : NopEntityBuilder<ShopItem>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ShopItem.ProductId)).AsInt32().ForeignKey<Product>();
        }
    }
}