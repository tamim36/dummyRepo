using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Models;
using Nop.Plugin.NopStation.FacebookShop.Domains;

namespace Nop.Plugin.NopStation.FacebookShop.Areas.Admin.Infrastructure
{
    public class FacebookShopMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;
        public FacebookShopMapperConfiguration()
        {
            CreateMap<ShopItem, ShopItemModel>()
                .ForMember(model => model.AvailableGenders, options => options.Ignore())
                .ForMember(model => model.AvailableAgeGroups, options => options.Ignore())
                .ForMember(model => model.AvailableProductConditions, options => options.Ignore())
                .ForMember(model => model.CustomProperties, options => options.Ignore());

            CreateMap<ShopItemModel, ShopItem>();

            CreateMap<FacebookShopSettings, ConfigurationModel>()
                .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore())
                .ForMember(model => model.AvailableLanguages, options => options.Ignore())
                .ForMember(model => model.AvailableCurrencies, options => options.Ignore());
            CreateMap<ConfigurationModel, FacebookShopSettings>();
        }
    }
}
