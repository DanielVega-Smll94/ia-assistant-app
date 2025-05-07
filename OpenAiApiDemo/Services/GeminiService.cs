using Newtonsoft.Json;
using OpenAiApiDemo.Controllers;
using System.Net.Http.Headers;
using System.Text;

namespace OpenAiApiDemo.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly string _apiUrlBeta;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["GeminiAi:ApiKey"];
            _apiUrl = config["GeminiAi:Api"];
            _apiUrlBeta = config["GeminiAi:ApiBeta"];
        }

        public async Task<string> GenerarRespuestaAsync(string prompt)
        {
            var url = $"{_apiUrl}gemini-1.5-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt },
                        }
                    }
                },
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Error Gemini: {response.StatusCode} - {response.ReasonPhrase}";
                try
                {
                    dynamic error = JsonConvert.DeserializeObject(responseContent);
                    errorMsg += $"\nMensaje: {error?.error?.message}";
                }
                catch { }

                throw new Exception(errorMsg);
            }

            dynamic result = JsonConvert.DeserializeObject(responseContent);
            return result?.candidates[0]?.content?.parts[0]?.text ?? "Sin respuesta";
        }

        public async Task<string> GenerarImagenRespuestaAsync(PromptDto dto)
        {
            var url = $"{_apiUrlBeta}gemini-2.0-flash-exp-image-generation:generateContent?key={_apiKey}";

            object requestBody;

            if (!string.IsNullOrEmpty(dto.ImagePath))
            {
                string base64Image = dto.ImagePath;//Convert.ToBase64String(imageData);

                requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = dto.Prompt },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = dto.MimeType ?? "image/png",
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    },
                    generationConfig = new { responseModalities = new[] { "TEXT", "IMAGE" } }
                };
            }
            else
            {
                requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = dto.Prompt }
                            }
                        }
                    },
                    generationConfig = new { responseModalities = new[] { "TEXT", "IMAGE" } } // Puedes omitir esto si solo esperas texto
                };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Error Gemini: {response.StatusCode} - {response.ReasonPhrase}";
                try
                {
                    dynamic error = JsonConvert.DeserializeObject(responseContent);
                    errorMsg += $"\nMensaje: {error?.error?.message}";
                }
                catch { }

                throw new Exception(errorMsg);
            }

            dynamic result = JsonConvert.DeserializeObject(responseContent);

            // **Aquí deberás procesar la respuesta para extraer el texto o la imagen**
            // La imagen estará probablemente en la estructura result?.candidates?[0]?.content?.parts?[0]?.inline_data?.data
            // y será una cadena Base64.

            // Por ahora, devolvemos la respuesta completa en JSON para que puedas inspeccionarla
            return responseContent;
        }

        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                // Add other supported types as needed
                default:
                    return "application/octet-stream"; // Default MIME type
            }
        }

    }
}
