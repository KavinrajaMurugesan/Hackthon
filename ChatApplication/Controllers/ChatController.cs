using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;

namespace ChatApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly HttpClient _httpClient;
    public ChatController(IHttpClientFactory httpClientFactory)
    {
         _httpClient=httpClientFactory.CreateClient();
    }
    [HttpPost("Please")]
    [AllowAnonymous]
    public async Task<IActionResult> GeneratePlantUml([FromBody] string userPrompt)
    {

        // Replace with your ChatGPT API URL and key
        var chatGptApiUrl = "https://chat.openai.com/";
        var chatGptApiKey = "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";

        // Replace with your PlantUML server URL
        var plantUmlServerUrl = "https://www.plantuml.com";

        var chatGptResponse = await CallChatGptApi(chatGptApiUrl, chatGptApiKey, userPrompt);

        if (chatGptResponse != null)
        {
            var plantUmlCode = chatGptResponse;

            var plantUmlImageUrl = await GeneratePlantUmlImage(plantUmlServerUrl, plantUmlCode);

            return Ok($"PlantUML image generated: {plantUmlImageUrl}");
        }
        else
        {
            return BadRequest($"Failed to get PlantUML code from ChatGPT. Status code: {chatGptResponse}");
        }

    }

    private async Task<string> CallChatGptApi(string apiUrl, string apiKey, string prompt)
    {
        string outputResult = "";
        var openai = new OpenAIAPI("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");
        CompletionRequest completionRequest = new CompletionRequest
        {
            Prompt = prompt,
            Model = OpenAI_API.Models.Model.DavinciText,
            MaxTokens = 1024
        };

        var completions = await openai.Completions.CreateCompletionAsync(completionRequest);

        foreach (var completion in completions.Completions)
        {
            outputResult += completion.Text;
        }
        return outputResult;
    }
    private async Task<string> GeneratePlantUmlImage(string plantUmlServerUrl, string plantUmlCode)
    {
        byte[] encodedBytes = Encoding.UTF8.GetBytes(plantUmlCode);

        // Convert the bytes to hexadecimal representation
        string hexRepresentation = BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();

        // Add "~h" at the start to indicate HEX format
        string encodedPlantuml = "~h" + hexRepresentation;

        Console.WriteLine(encodedPlantuml);
        var plantUmlUrl = $"{plantUmlServerUrl}/plantuml/png/{encodedPlantuml}";

        var response = await _httpClient.GetAsync(plantUmlUrl);

        if (response.IsSuccessStatusCode)
        {
            var imageContent = await response.Content.ReadAsByteArrayAsync();

            return Convert.ToBase64String(imageContent);
        }
        else
        {
            throw new BadHttpRequestException($"Failed to fetch PlantUML image. Status code: {response.StatusCode}");
        }
    }
}
