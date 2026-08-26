using System.Net.Http.Headers;
using System.Text.Json;
using DialogueModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var app = builder.Build();

/*
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();
*/

string requestUri = "https://openrouter.ai/api/v1/chat/completions";
string backendUrl = "http://localhost:3000"; // TODO: Cambiar por el de verdad

app.MapPost("/api/dialogue", async (DialogueRequest request, IHttpClientFactory httpClientFactory, IConfiguration configuration) =>
{
    string apiKey = configuration["OpenRouter:ApiKey"] ?? throw new InvalidOperationException("OpenRouter API key missing.");

    string model = configuration["OpenRouter:Model"] ?? throw new InvalidOperationException("OpenRouter model is missing.");

    string systemPrompt = """
    You are a customer in a sales-training dialogue.

    Stay in character as the customer.
    React naturally to the selected answer.
    Do not give a score.
    Do not reveal whether the answer is correct.
    Keep your response below 60 words.
    Return only the customer's dialogue response.
    """;

    string userPrompt = $"""
    Scenario context:
    {request.ScenarioContext}

    Customer persona:
    {request.CustomerPersona}

    Question-specific context:
    {request.QuestionDialogueContext}

    Question:
    {request.QuestionText}

    Selected answer:
    {request.SelectedAnswer}
    """;

    var messages = new List<object>
    {
        new {
            role = "system",
            content = systemPrompt
        }
    };

    foreach (DialogueMessage message in request.ConversationHistory)
    {
        var newMessage = new
        {
            role = message.Role,
            content = message.Content
        };
        messages.Add(newMessage);
    }

    var userMessage = new
    {
        role = "user",
        content = userPrompt
    };
    messages.Add(userMessage);

    var payload = new // TODO: Checkear que ondas con esto xd
    {
        model = model,
        messages = messages,
        temperature = 0.4 
    };

    HttpClient httpClient = httpClientFactory.CreateClient();

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

    HttpResponseMessage apiResponse = await httpClient.PostAsJsonAsync(requestUri, payload);

    string responseBody = await apiResponse.Content.ReadAsStringAsync();

    if (!apiResponse.IsSuccessStatusCode)
    {
        return Results.Problem(detail: responseBody, statusCode: (int) apiResponse.StatusCode);
    }

    OpenRouterResponse? openRouterResult = JsonSerializer.Deserialize<OpenRouterResponse>(responseBody, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});

    if (openRouterResult == null || openRouterResult.Choices.Count == 0)
    {
        return Results.Problem("OpenRouter returned no usable response.");
    }

    DialogueResponse response = new()
    {
        ResponseId = openRouterResult.Id,
        Model = openRouterResult.Model,
        ReplyText = openRouterResult.Choices[0].Message.Content
    };

    return Results.Ok(response);
});

app.Run(backendUrl);