using AmazonProductBrandCheck.ViewModels;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AmazonProductBrandCheck.Repository
{
    public class AlProductBrandAnalysis : IAlProductBrandAnalysis
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey = "YOUR-OPENAL-API-KEY-HERE"; 

        public AlProductBrandAnalysis(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductBrandResult>> AnalyzeProductsAsync(List<Product> products)
        {
            var results = new List<ProductBrandResult>();

            foreach (var product in products)
            {
                try
                {
                    byte[] imageBytes = await _httpClient.GetByteArrayAsync(product.PhotoUrl);
                    string base64Image = Convert.ToBase64String(imageBytes);

                    var requestBody = new
                    {
                        model = "gpt-4o",
                        messages = new[]
                        {
                            new
                            {
                                role = "user",
                                content = new object[]
                                {
                                    new {
                                        type = "image_url",
                                        image_url = new {
                                            url = $"data:image/jpeg;base64,{base64Image}"
                                        }
                                    },
                                    new {
                                        type = "text",
                                        text = "Look at the product image. Based on its appearance, does it look branded (e.g. has a logo or brand name), unbranded (generic with no logo), or is it unclear? Answer with one of: Branded, Unbranded, Unknown. Note: I want to buy and sell these products from China. If the product is bought from China and has a small logo or name on it, mark it as unbranded."
                                    }
                                }
                            }
                        },
                        max_tokens = 100
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
                        Headers =
                        {
                            { "Authorization", $"Bearer {_openAiApiKey}" }
                        },
                        Content = content
                    };

                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await _httpClient.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    BrandStatus status = BrandStatus.Unknown;
                    string resultText = "No response";

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonDoc = JsonDocument.Parse(responseBody);
                        var answer = jsonDoc.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

                        resultText = answer;
                        var normalized = resultText.Trim().ToLowerInvariant();

                        if (normalized.Contains("unbranded"))
                            status = BrandStatus.Unbranded;
                        else if (normalized.Contains("branded"))
                            status = BrandStatus.Branded;
                        else if (normalized.Contains("unknown") || normalized.Contains("unclear") || normalized.Contains("not sure"))
                            status = BrandStatus.Unknown;
                        else
                            status = BrandStatus.Unknown;
                    }
                    else
                    {
                        resultText = $"OpenAI API error: {response.StatusCode}, Body: {responseBody}";
                    }

                    results.Add(new ProductBrandResult
                    {
                        Asin = product.Asin,
                        BrandStatus = status,
                        RawResponse = resultText
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new ProductBrandResult
                    {
                        Asin = product.Asin,
                        BrandStatus = BrandStatus.Unknown,
                        RawResponse = "Error: " + ex.Message
                    });
                }
            }

            return results;
        }
    }
}
