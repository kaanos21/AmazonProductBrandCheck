using AmazonProductBrandCheck.ViewModels;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AmazonProductBrandCheck.Repository
{
    public class AlProductBrandAnalysis : IAlProductBrandAnalysis
    {
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey = "AIzaSyC6IFWFl_4EgKF4VoearSNo5nfsWvUoCjo";

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
                    // Fotoğrafı indir ve base64'e çevir
                    byte[] imageBytes = await _httpClient.GetByteArrayAsync(product.PhotoUrl);
                    string base64Image = Convert.ToBase64String(imageBytes);

                    // Gemini API isteği için body
                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new {
                                parts = new object[]
                                {
                                    new {
                                        inlineData = new {
                                            mimeType = "image/jpeg",
                                            data = base64Image
                                        }
                                    },
                                    new {
                                        text = "Look at the product image. Based on its appearance, does it look branded, unbranded, or is it unclear? Answer with one of: Branded, Unbranded, Unknown."
                                    }
                                }
                            }
                        }
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Güncel model endpoint'i
                    var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}";
                    var response = await _httpClient.PostAsync(endpoint, content);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    BrandStatus status = BrandStatus.Unknown;
                    string resultText = "No response";

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonDoc = JsonDocument.Parse(responseBody);
                        if (jsonDoc.RootElement.TryGetProperty("candidates", out var candidates) &&
                            candidates.GetArrayLength() > 0 &&
                            candidates[0].TryGetProperty("content", out var contentElement) &&
                            contentElement.TryGetProperty("parts", out var parts) &&
                            parts.GetArrayLength() > 0 &&
                            parts[0].TryGetProperty("text", out var textElement))
                        {
                            resultText = textElement.GetString();

                            status = resultText.ToLower() switch
                            {
                                var s when s.Contains("branded") => BrandStatus.Branded,
                                var s when s.Contains("unbranded") => BrandStatus.Unbranded,
                                _ => BrandStatus.Unknown
                            };
                        }
                        else
                        {
                            resultText = "No text content found in Gemini response.";
                        }
                    }
                    else
                    {
                        resultText = $"Gemini API error: {response.StatusCode}, Body: {responseBody}";
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
