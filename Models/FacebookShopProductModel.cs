using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Web.Models.Media;

namespace Nop.Plugin.NopStation.FacebookShop.Models
{
    public partial class FacebookShopProductModel
    {
        public FacebookShopProductModel()
        {
            DefaultPictureModel = new PictureModel();
            ProductPrice = new ProductPriceModel();
        }
        
        // pictures
        public PictureModel DefaultPictureModel { get; set; }
        //additional images
        public IList<PictureModel> PictureModels { get; set; }



        public int ProductId { get; set; }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }

        public string Brand { get; set; }
        public string GoogleCategory { get; set; }


        public string Sku { get; set; }
        public string StockAvailability { get; set; }
        public string Condition { get; set; }

        public ProductPriceModel ProductPrice { get; set; }

        public partial record ProductPriceModel
        {
            /// <summary>
            /// The currency (in 3-letter ISO 4217 format) of the offer price 
            /// </summary>
            public string CurrencyCode { get; set; }

            public string OldPrice { get; set; }

            public string Price { get; set; }
            public string PriceWithDiscount { get; set; }
            public decimal PriceValue { get; set; }

            public bool CustomerEntersPrice { get; set; }

            public bool CallForPrice { get; set; }

            public int ProductId { get; set; }

            public bool HidePrices { get; set; }

            //rental
            public bool IsRental { get; set; }
            public string RentalPrice { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
            /// <summary>
            /// PAngV baseprice (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }
        }

    }
    
}
