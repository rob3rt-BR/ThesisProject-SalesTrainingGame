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
                throw new OpenRouterRateLimitException("OpenRouter rate limit reached.");
            }
            throw new HttpRequestException($"OpenRouter returned {(int)apiResponse.StatusCode}.");
        }

        OpenRouterResponse? openRouterResult = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody);

        if (openRouterResult == null || openRouterResult.Choices.Count == 0 || openRouterResult.Choices[0].Message == null)
        {
            throw new InvalidOperationException("OpenRouter returned an empty response.");
        }

        var choice = openRouterResult.Choices[0];
        logger.LogInformation("AI finish reason for {QuestionId}: {FinishReason}", request.QuestionId, choice.FinishReason);

        string rawResultContent = openRouterResult.Choices[0].Message.Content;
        string replyText = RemoveReplyQuotationmarks(rawResultContent);

        logger.LogWarning("Raw result content for {QuestionId}: {Content}", request.QuestionId, rawResultContent);

        if (string.IsNullOrWhiteSpace(replyText))
        {
            throw new InvalidOperationException("The AI returned an empty customer response.");
        }

        OpenRouterUsage? usage = openRouterResult.Usage;

        logger.LogInformation("OpenRouter request completed. Model: {Model}, " +
        "Scenario: {ScenarioId}, Module: {ModuleId}, Question: {QuestionId}, " +
        "PromptTokens: {PromptTokens}, CompletionTokens: {CompletionTokens}, " +
        "ResponseTimeMs: {ResponseTimeMs}" +
        "TraineeDialogueText: {TraineeDialogueText}" +
        "RawReplyText: {ReplyText}",
        openRouterResult.Model,
        request.ScenarioId,
        request.ModuleId,
        request.QuestionId,
        usage?.PromptTokens ?? 0,
        usage?.CompletionTokens ?? 0,
        stopWatch.ElapsedMilliseconds,
        request.TraineeDialogueText,
        rawResultContent);

        return new DialogueResponse
        {
            ResponseId = openRouterResult.Id,
            Model = openRouterResult.Model,
            PromptTokens = usage?.PromptTokens ?? 0,
            CompletionTokens = usage?.CompletionTokens ?? 0,
            ResponseTimeMilliseconds = stopWatch.ElapsedMilliseconds,
            
            ReplyText = replyText
        };
    }

    private static List<object> BuildMessages(DialogueRequest request)
    {
        string systemPrompt = """
        Du bist der Kunde in einem deutschsprachigen Sales-Training.

        Antworte ausschließlich auf Deutsch.
        Bleibe vollständig in deiner Kundenrolle und berücksichtige den Szenariokontext sowie die Kundenpersona.
        Verhalte dich professionell, glaubwürdig und leicht interaktiv.
        Übernimm nicht die Rolle eines Trainers.

        Reagiere natürlich und konkret auf die vorbereitete Aussage des Teilnehmers, ohne sie wörtlich zu wiederholen.
        Beziehe dich auf die aktuelle Situation und die Kundenpersona.
        Deine Reaktion darf positiv, zurückhaltend, irritiert oder kritisch ausfallen.

        Die Kundenreaktion darf ausschließlich aus Aussagen bestehen.
        Stelle innerhalb dieser Reaktion keine zusätzlichen, rhetorischen oder klärenden Fragen.

        Wenn der aktuelle Dialogkontext eine Überleitungsfrage vorsieht, muss deine Antwort genau eine kurze und natürliche Überleitungsfrage enthalten.
        Diese Frage muss der letzte Satz der Antwort sein und darf nicht weggelassen werden.

        Wenn der Dialogkontext keine Überleitungsfrage vorsieht, stelle überhaupt keine Frage.

        Erwähne keine Trainingsfragen, Antwortoptionen, Lernziele oder nächsten Module.
        Verrate keine richtige Antwort und bewerte weder die Aussage noch den Punktestand.

        Halte die Antwort kurz.
        Gib ausschließlich den fertigen Kundentext zurück.
        Setze den gesamten Text nicht in Anführungszeichen.
        Verwende keine Markdown-Formatierung und kein Präfix wie „Kunde:“.
        """;
        // Stelle innerhalb dieser Reaktion keine zusätzlichen, rhetorischen oder klärenden Fragen und verwende dort kein Fragezeichen.
        // Die gesamte Antwort darf höchstens ein Fragezeichen enthalten.

        string requestProperties = $"""
        Aktueller Szenariokontext: {request.ScenarioContext}

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
        Aktueller Dialogkontext:
        {request.QuestionDialogueContext}

        Aussage des Teilnehmers:
        {request.TraineeDialogueText}

        Reagiere als Kunde auf die Aussage.
        Falls der Dialogkontext eine Überleitungsfrage vorsieht, stelle genau eine als letzten Satz.
        Die Reaktion davor darf keine Frage enthalten.
        Falls keine Überleitungsfrage vorgesehen ist, stelle keine Frage.
        
        Gib nur den fertigen Kundentext zurück, ohne Anführungszeichen um die gesamte Antwort.
        """;

        var userMessage = new
        {
            role = "user",
            content = userRequestProperties
        };
        messages.Add(userMessage);
        return messages;
    }

    private static string RemoveReplyQuotationmarks(string content)
    {
        string replyText = content.Trim();

        if ((replyText.StartsWith("\"") && replyText.EndsWith("\"")) || (replyText.StartsWith("„") && replyText.EndsWith("“")))
        {
            replyText = replyText[1..^1].Trim();
        }
        return replyText;
    }
}