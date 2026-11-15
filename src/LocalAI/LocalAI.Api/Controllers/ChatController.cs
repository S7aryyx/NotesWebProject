using LocalAiApi.Models;
using LocalAiApi.Services;

using Microsoft.AspNetCore.Mvc;
//Папка Controllers -> Файл : ChatController.cs
namespace LocalAiApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController(PromptProvider promptProvider,
        OllamaService ollamaService,
        AiAccessTokenValidator accessTokenValidator) : ControllerBase
    {
        //health 
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok();
        }
        //prompts 
        [HttpGet("prompts")]
        public IActionResult GetPrompts()
        {
            return Ok(promptProvider.GetKeys());
        }

        //chat
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request,CancellationToken cancellationToken)
        {
            string? authorization = Request.Headers["Authorization"].ToString();
            string? token = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization.Substring("Bearer ".Length).Trim()
                : null;

            if (!accessTokenValidator.IsValid(token))
                return Unauthorized("Для ИИ-анализа требуется активная подписка или единовременный доступ.");

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Текст заметки пустой.");

            string? prompt = promptProvider.GetValue(request.Prompt);

            if (string.IsNullOrWhiteSpace(prompt))
                return BadRequest("Указанный промпт не найден.");

            try
            {
                string answer = await ollamaService.ChatAsync(
                    request.Message,
                    prompt,
                    cancellationToken
                );

                return Ok(answer);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, "Ollama недоступна.");
            }
            catch (InvalidOperationException)
            {
                return StatusCode(502, "ИИ вернула некорректный ответ.");
            }
        }

    }
}
