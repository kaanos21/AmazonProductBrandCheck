using AmazonProductBrandCheck.ViewModels;
using System.Collections.Generic;
using System.IO;

namespace AmazonProductBrandCheck.Repository
{
    public interface IReadProductsFromExcel
    {
        List<Product> ReadProducts(Stream excelStream);
    }
}
