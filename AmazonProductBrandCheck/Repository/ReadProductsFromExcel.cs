using AmazonProductBrandCheck.ViewModels;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.IO;

namespace AmazonProductBrandCheck.Repository
{
    public class ReadProductsFromExcel : IReadProductsFromExcel
    {
        public List<Product> ReadProducts(Stream excelStream)
        {
            var products = new List<Product>();

            using var workbook = new XLWorkbook(excelStream);
            var worksheet = workbook.Worksheet(1); // İlk sayfa

            int row = 2; // Başlık varsa 2’den başla, yoksa 1’den başla

            while (!worksheet.Cell(row, 2).IsEmpty() && !worksheet.Cell(row, 3).IsEmpty())
            {
                string photoUrl = worksheet.Cell(row, 2).GetString().Trim(); // B sütunu
                string asin = worksheet.Cell(row, 3).GetString().Trim();    // C sütunu

                products.Add(new Product
                {
                    Asin = asin,
                    PhotoUrl = photoUrl
                });

                row++;
            }

            return products;
        }
    }
}
