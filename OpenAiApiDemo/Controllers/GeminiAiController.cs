using Microsoft.AspNetCore.Mvc;
using OpenAiApiDemo.Services;

namespace OpenAiApiDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiAiController : ControllerBase
    {
        private readonly GeminiService _geminiService;

        public GeminiAiController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PromptDto dto)
        {
            var respuesta = await _geminiService.GenerarRespuestaAsync(dto.Prompt);
            return Ok(respuesta);
        }

        [HttpPost("generar-imagenes")]
        public async Task<IActionResult> GenerarImagenPost([FromBody] PromptDto dto)
        {
            var respuesta = await _geminiService.GenerarImagenRespuestaAsync(dto);
            return Ok(respuesta);
        }
    }
}
