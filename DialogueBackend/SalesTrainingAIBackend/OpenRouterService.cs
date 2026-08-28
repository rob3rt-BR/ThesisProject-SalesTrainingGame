using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DialogueModels;
using OpenRouterResponseModels;

public sealed class OpenRouterService
{
    private readonly HttpClient httpClient;
    private readonly IConfiguration configuration;
    private readonly ILogger<OpenRouterService> logger;
    public OpenRouterService(HttpClient httpClient, IConfiguration configuration, ILogger<OpenRouterService> logger)
    {
        this.httpClient = httpClient;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<DialogueResponse> GenerateDialogueAsync(DialogueRequest request, CancellationToken cancellationToken)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenRouter API Key environment variable is missing");
        }
        
        var model = configuration["OpenRouter:Model"] ?? throw new InvalidOperationException("OpenRouter model is missing.");
        var maxTokens = int.TryParse(configuration["OpenRouter:MaxTokens"], out var configuredMaxTokens) ? configuredMaxTokens : 150;
        var temperature = double.TryParse(configuration["OpenRouter:Temperature"], out var configuredTemperature) ? configuredTemperature : 0.3;

        var messages = BuildMessages(request);
        var payload = new
        {
            model,
            messages,
            max_tokens = maxTokens,
            temperature,
            provider = new
            {
                allow_fallbacks = true
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Headers.Add("HTTP-Referer", "http://localhost");
        httpRequest.Headers.Add("X-Title", "Sales Topics Training Game");

        var json = JsonSerializer.Serialize(payload);
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var stopWatch = Stopwatch.StartNew();

        HttpResponseMessage apiResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await apiResponse.Content.ReadAsStringAsync(cancellationToken);

        stopWatch.Stop();

        if (!apiResponse.IsSuccessStatusCode)
        {
            logger.LogWarning("OpenRouter request failed. Status: {StatusCode}, " +
            "Scenario: {ScenarioId}, Module: {ModuleId}, Question: {QuestionId}, " +
            "Response: {Response}",
            apiResponse.StatusCode,
            request.ScenarioId,
            request.ModuleId,
            request.QuestionId,
            responseBody);

            if (apiResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new InvalidOperationException("OpenRouter rate limit reached.");
            }
            throw new HttpRequestException($"OpenRouter returned {(int)apiResponse.StatusCode}.");
        }

        OpenRouterResponse? openRouterResult = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody);

        if (openRouterResult == null || openRouterResult.Choices.Count == 0 || openRouterResult.Choices[0].Message == null)
        {
            throw new InvalidOperationException("OpenRouter returned an empty response.");
        }

        OpenRouterUsage? usage = openRouterResult.Usage;

        logger.LogInformation("OpenRouter request completed. Model: {Model}, " +
        "Scenario: {ScenarioId}, Module: {ModuleId}, Question: {QuestionId}, " +
        "PromptTokens: {PromptTokens}, CompletionTokens: {CompletionTokens}, " +
        "ResponseTimeMs: {ResponseTimeMs}" +
        "ReplyText: {ReplyText}",
        openRouterResult.Model,
        request.ScenarioId,
        request.ModuleId,
        request.QuestionId,
        usage?.PromptTokens ?? 0,
        usage?.CompletionTokens ?? 0,
        stopWatch.ElapsedMilliseconds,
        openRouterResult.Choices[0].Message.Content);

        return new DialogueResponse
        {
            ResponseId = openRouterResult.Id,
            Model = openRouterResult.Model,
            PromptTokens = usage?.PromptTokens ?? 0,
            CompletionTokens = usage?.CompletionTokens ?? 0,
            ResponseTimeMilliseconds = stopWatch.ElapsedMilliseconds,
            ReplyText = openRouterResult.Choices[0].Message.Content
        };
    }

    private static List<object> BuildMessages(DialogueRequest request)
    {
        string systemPrompt = """
        Du bist ein Kunde in einem Sales-Training.

        Antworte ausschließlich auf Deutsch.
        Reagiere natürlich und kurz auf die ausgewählte Antwort des Trainingsteilnehmers.
        Bleibe in deiner Kundenrolle und berücksichtige den aktuellen Szenario-Kontext.
        Verhalte dich professionell, aber leicht interaktiv und spielerisch.
        Gib nicht die richtige Antwort auf die Frage bekannt.
        Bewerte weder die Antwort noch den Punktestand des Teilnehmers.
        Übernimm nicht die Rolle eines Trainers.
        Antworte nur mit der Reaktion des Kunden.
        """;

        string requestProperties = $"""
        Szenario-Kontext: {request.ScenarioContext}
        Kundenpersona: {request.CustomerPersona}
        """;

        var messages = new List<object>();
        var systemMessage = new
        {
            role = "system",
            content = systemPrompt
        };
        var systemPropertiesMessage = new
        {
            role = "system",
            content = requestProperties
        };
        messages.Add(systemMessage);
        messages.Add(systemPropertiesMessage);

        foreach (DialogueMessage historyMessage in request.ConversationHistory)
        {
            if (historyMessage.Role is "user" or "assistant")
            {
                var newMessage = new
                {
                    role = historyMessage.Role,
                    content = historyMessage.Content
                };
                messages.Add(newMessage);
            }
        }

        string userRequestProperties = $"""
        Dialogkontext zur aktuellen Frage: {request.QuestionDialogueContext}
        Frage: {request.QuestionText}
        Ausgewählte Antwort: {request.SelectedAnswer}
        Reagiere als Kunde auf die ausgewählte Antwort.
        """;

        var userMessage = new
        {
            role = "user",
            content = userRequestProperties
        };
        messages.Add(userMessage);
        return messages;
    }
}