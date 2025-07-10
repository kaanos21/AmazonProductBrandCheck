using AmazonProductBrandCheck.ViewModels;

namespace AmazonProductBrandCheck.Repository
{
    public interface IAlProductBrandAnalysis
    {
        Task<List<ProductBrandResult>> AnalyzeProductsAsync(List<Product> products);
    }
}
