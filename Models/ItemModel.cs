using CsvHelper.Configuration.Attributes;

namespace Nop.Plugin.NopStation.FacebookShop.Models
{
    public class ItemModel
    {
        #region Required fields for products

        [Name("id")]
        public string Id { get; set; }

        [Name("title")]
        public string Title { get; set; }

        [Name("description")]
        public string Description { get; set; }

        [Name("availability")]
        public string Availability { get; set; }

        [Name("condition")]
        public string Condition { get; set; }

        [Name("price")]
        public string Price { get; set; }

        [Name("link")]
        public string Link { get; set; }

        [Name("image_link")]
        public string Image_Link { get; set; }

        [Name("brand")]
        public string Brand { get; set; }

        #endregion Required fields for products

        #region Optional fields for products

        [Name("rich_text_description")]
        public string Rich_Text_Description { get; set; }

        [Name("sale_price")]
        public string Sale_Price { get; set; }

        [Name("sale_price_effective_date")]
        public string Sale_Price_Effective_Date { get; set; }

        [Name("item_group_id")]
        public string Item_Group_Id { get; set; }

        [Name("additional_image_link")]
        public string Additional_Image_Link { get; set; }

        [Name("color")]
        public string Color { get; set; }

        [Name("gender")]
        public string Gender { get; set; }

        [Name("size")]
        public string Size { get; set; }

        [Name("age_group")]
        public string Age_Group { get; set; }

        [Name("material")]
        public string Material { get; set; }

        [Name("pattern")]
        public string Pattern { get; set; }

        [Name("shipping")]
        public string Shipping { get; set; }

        [Name("shipping_weight")]
        public string Shipping_Weight { get; set; }

        [Name("delete")]
        public string Delete { get; set; }

        #endregion Optional fields for products

        #region Additional required fields for checkout

        [Name("quantity_to_sell_on_facebook")]
        public string Quantity_To_Sell_On_Facebook { get; set; }

        [Name("fb_product_category")]
        public string Fb_Product_Category { get; set; }

        [Name("google_product_category")]
        public string Google_Product_Category { get; set; }

        #endregion Additional required fields for checkout
    }
}