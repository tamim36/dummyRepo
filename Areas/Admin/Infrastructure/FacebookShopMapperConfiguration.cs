using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Models;
using NopStation.Plugin.Misc.FacebookShop.Domains;

namespace NopStation.Plugin.Misc.FacebookShop.Areas.Admin.Infrastructure
{
    public class FacebookShopMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public int Order => 1;
        public FacebookShopMapperConfiguration()
        {
            CreateMap<ShopItem, ShopItemModel>()
                .ForMember(model => model.AvailableGenders, options => options.Ignore())
                .ForMember(model => model.AvailableAgeGroups, options => options.Ignore())

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
