using Microsoft.AspNetCore.Mvc;
using OpenAiApiDemo.Services;

namespace OpenAiApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpenAiController : ControllerBase
    {
        private readonly OpenAiService _openAiService;

        public OpenAiController(OpenAiService openAiService)
        {
            _openAiService = openAiService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PromptDto dto)
        {
            var respuesta = await _openAiService.GenerarRespuestaAsync(dto.Prompt, dto.model);
            return Ok(respuesta);
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var modelos = await _openAiService.ObtenerModelosAsync();
            return Ok(modelos);
        }

        [HttpGet("obetner-modelos-tratados")]
        public async Task<IActionResult> GetFilter()
        {
            var modelos = await _openAiService.ObtenerModelosTratadaAsync();
            return Ok(modelos);
        }

        [HttpPost("generar-imagen")]
        public async Task<IActionResult> GenerarImagenDalle([FromBody] PromptDto dto)
        {
            try
            {
                var url = await _openAiService.GenerarImagenDalleAsync(dto.Prompt);
                return Ok(new { imageUrl = url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("descargar-imagen")]
        public async Task<IActionResult> DescargarImagen([FromQuery] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("URL inválida.");

            try
            {
                (byte[] contenido, string contentType) = await _openAiService.DescargarImagenAsync(url);
                return File(contenido, contentType, "imagen_generada.png");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al descargar imagen: {ex.Message}");
            }
        }
    }

    public class PromptDto
    {
        public string Prompt { get; set; }
        public string? model { get; set; }
        public string? ImagePath { get; set; }
        public string? MimeType { get; set; }

    }
}
