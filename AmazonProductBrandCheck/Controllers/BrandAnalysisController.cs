using AmazonProductBrandCheck.Repository;
using AmazonProductBrandCheck.ViewModels;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BrandAnalysisController : ControllerBase
{
    private readonly IReadProductsFromExcel _excelReader;
    private readonly IAlProductBrandAnalysis _brandAnalysis;

    public BrandAnalysisController(IReadProductsFromExcel excelReader, IAlProductBrandAnalysis brandAnalysis)
    {
        _excelReader = excelReader;
        _brandAnalysis = brandAnalysis;
    }

    [HttpPost("upload-excel")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Excel dosyası seçilmedi.");

        List<Product> products;

        using (var stream = file.OpenReadStream())
        {
            products = _excelReader.ReadProducts(stream);
        }

        var results = await _brandAnalysis.AnalyzeProductsAsync(products);

        return Ok(results);
    }
}
