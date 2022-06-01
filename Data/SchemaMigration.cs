using FluentMigrator;
using Nop.Data.Migrations;
using Nop.Plugin.NopStation.FacebookShop.Domains;

namespace Nop.Plugin.NopStation.FacebookShop.Data
{
    [SkipMigrationOnUpdate]
    [NopMigration("2021/03/31 08:15:54:1687511", "NopStation.FacebookShop base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        public override void Up()
        {
            _migrationManager.BuildTable<ShopItem>(Create);
        }
    }
}