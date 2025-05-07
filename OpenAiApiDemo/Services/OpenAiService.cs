using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace OpenAiApiDemo.Services
{
    public class ChatModelDto
    {
        public string Id { get; set; }       
        public string Nombre { get; set; }    
        public string Emoji { get; set; }
    }
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        public OpenAiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            // leer appsettings.json
            _apiKey = config["OpenAi:ApiKey"];
            _apiUrl = config["OpenAi:Api"];
        }

        public async Task<List<string>> ObtenerModelosAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener modelos: {response.StatusCode} - {content}");

            var modelos = new List<string>();
            var json = JObject.Parse("{\"data\":" + content + "}");
            var modelosData = JObject.Parse(content)["data"];

            foreach (var modelo in modelosData)
            {
                modelos.Add(modelo["id"]?.ToString() ?? "");
            }

            return modelos = modelos.OrderBy(m => m).ToList();
        }

        public async Task<List<ChatModelDto>> ObtenerModelosTratadaAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}models");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener modelos: {response.StatusCode} - {content}");

            var json = JObject.Parse(content);
            var modelosRaw = json["data"]
                .Where(m => m["id"] != null && m["created"] != null)
                .Select(m => new
                {
                    Id = m["id"]!.ToString(),
                    Created = (long)m["created"]!
                })
                .ToList();

            // Filtrar solo chat models y agrupar por tipo base
            var chatModelos = modelosRaw
                .Where(m => m.Id.StartsWith("gpt-") &&
                            !m.Id.Contains("tts") &&
                            !m.Id.Contains("whisper") &&
                            !m.Id.Contains("image") &&
                            !m.Id.Contains("embedding") &&
                            !m.Id.Contains("search") &&
                            !m.Id.Contains("transcribe"))
                .GroupBy(m => ExtraerTipoBase(m.Id))
                .Select(grupo =>
                {
                    var modeloMasReciente = grupo.OrderByDescending(x => x.Created).First();
                    return new ChatModelDto
                    {
                        Id = modeloMasReciente.Id,
                        Nombre = FormatearNombre(modeloMasReciente.Id),
                        Emoji = "🤖"
                    };
                })
                .OrderBy(m => m.Nombre)
                .ToList();

            return chatModelos;
        }

        private string ExtraerTipoBase(string id)
        {
            // Ejemplo: "gpt-4o-2024-12-17" -> "gpt-4o"
            var partes = id.Split('-');
            if (partes.Length >= 2)
                return partes[0] + "-" + partes[1]; // "gpt-4o"
            return id;
        }

        private string FormatearNombre(string id)
        {
            if (id.StartsWith("gpt-4o")) return "GPT-4o";
            if (id.StartsWith("gpt-4.5")) return "GPT-4.5 Preview";
            if (id.StartsWith("gpt-4-turbo")) return "GPT-4 Turbo";
            if (id.StartsWith("gpt-4")) return "GPT-4";
            if (id.StartsWith("gpt-3.5")) return "GPT-3.5 Turbo";
            return id;
        }

        public async Task<string> GenerarRespuestaAsync(string prompt, string modelo)
        {
            prompt = prompt.Replace("\r\n", " ").Replace("\n", " ");
            prompt = prompt.Trim();

            var requestBody = new
            {
                model = modelo,//"gpt-3.5-turbo",
                messages = new[] {
                new { role = "user", content = prompt }
            }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();


            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"La IA no pudo responder. Código: {response.StatusCode} - {response.ReasonPhrase}";
                try
                {
                    dynamic error = JsonConvert.DeserializeObject(responseBody);
                    errorMsg += $"\nMensaje: {error?.error?.message}";
                }
                catch { }

                throw new Exception(errorMsg);
            }

            dynamic result = JsonConvert.DeserializeObject(responseBody);
            return result?.choices[0]?.message?.content ?? "No response";
        }

        public async Task<string> GenerarImagenDalleAsync(string prompt)
        {
            var requestBody = new
            {
                model = "dall-e-3",
                prompt = prompt,
                n = 1,
                size = "1024x1024"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiUrl}images/generations");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = $"Error al generar imagen: {response.StatusCode} - {response.ReasonPhrase}";
                try
                {
                    dynamic error = JsonConvert.DeserializeObject(responseBody);
                    errorMsg += $"\nMensaje: {error?.error?.message}";
                }
                catch { }

                throw new Exception(errorMsg);
            }

            dynamic result = JsonConvert.DeserializeObject(responseBody);
            return result?.data?[0]?.url ?? throw new Exception("No se generó ninguna imagen.");
        }

        public async Task<(byte[] contenido, string contentType)> DescargarImagenAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al obtener imagen. Código: {response.StatusCode}");

            var content = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            return (content, contentType);
        }
    }
}
