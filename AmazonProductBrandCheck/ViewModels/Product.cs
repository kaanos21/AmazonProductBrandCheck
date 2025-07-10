namespace AmazonProductBrandCheck.ViewModels
{
    public enum BrandStatus
    {
        Unknown = 1,
        Branded = 2,
        Unbranded = 3
    }

    public class Product
    {
        public string Asin { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class ProductBrandResult
    {
        public string Asin { get; set; }
        public BrandStatus BrandStatus { get; set; }
        public string RawResponse { get; set; }
    }
}
