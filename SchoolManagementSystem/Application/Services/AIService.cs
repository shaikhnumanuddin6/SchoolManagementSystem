
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.Application.Services;

public class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IConfiguration _configuration;

    public AIService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "Groq API key is missing. Configure it using User Secrets.");

        _httpClient.BaseAddress =
            new Uri("https://api.groq.com/openai/v1/");

        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<string> GetResponseAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException(
                "Prompt cannot be empty.",
                nameof(prompt));
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "chat/completions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        var model =
            _configuration["OpenAI:Model"]
            ?? "llama-3.3-70b-versatile";

        request.Content = JsonContent.Create(new
        {
            model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content =
                        "You are a helpful AI assistant for a school management system. " +
                        "Give clear, practical, and professional answers."
                },
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.7
        });

        using var response =
            await _httpClient.SendAsync(request);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage =
                "Groq did not provide additional error details.";

            var errorCode = string.Empty;

            try
            {
                using var errorDocument =
                    JsonDocument.Parse(responseBody);

                if (errorDocument.RootElement.TryGetProperty(
                        "error", out var error))
                {
                    if (error.ValueKind == JsonValueKind.Object &&
                        error.TryGetProperty(
                            "message", out var message))
                    {
                        errorMessage =
                            message.GetString() ?? errorMessage;
                    }
                    else if (error.ValueKind == JsonValueKind.String)
                    {
                        errorMessage =
                            error.GetString() ?? errorMessage;
                    }

                    if (error.ValueKind == JsonValueKind.Object &&
                        error.TryGetProperty(
                            "code", out var code))
                    {
                        errorCode = code.ToString();
                    }
                }
            }
            catch (JsonException)
            {
                // Keep the fallback message for non-JSON responses.
            }

            var details =
                $"Groq API error: {(int)response.StatusCode} " +
                $"({response.StatusCode}). {errorMessage}";

            if (!string.IsNullOrWhiteSpace(errorCode))
            {
                details += $" Error code: {errorCode}.";
            }

            throw new HttpRequestException(details);
        }

        using var document =
            JsonDocument.Parse(responseBody);

        if (document.RootElement.TryGetProperty(
                "choices", out var choices) &&
            choices.ValueKind == JsonValueKind.Array &&
            choices.GetArrayLength() > 0)
        {
            var firstChoice = choices[0];

            if (firstChoice.TryGetProperty(
                    "message", out var message) &&
                message.TryGetProperty(
                    "content", out var content))
            {
                var answer = content.GetString();

                if (!string.IsNullOrWhiteSpace(answer))
                {
                    return answer;
                }
            }
        }

        throw new InvalidOperationException(
            "Groq returned a response without readable text.");
    }
}
