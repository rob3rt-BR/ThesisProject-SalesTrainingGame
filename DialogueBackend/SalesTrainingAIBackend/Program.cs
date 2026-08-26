using System.Net.Http.Headers;
using System.Text.Json;
using DialogueModels;
using OpenRouterResponseModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<OpenRouterService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
    client.Timeout = TimeSpan.FromSeconds(45);
});

var app = builder.Build();

string backendUrl = "http://localhost:3000"; // TODO: Cambiar por el de verdad

app.MapPost("/api/dialogue", async (DialogueRequest request, OpenRouterService openRouterService, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await openRouterService.GenerateDialogueAsync(request, cancellationToken);

        return Results.Ok(result);
    } 
    catch (InvalidOperationException)
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    } 
    catch (HttpRequestException)
    {
        return Results.StatusCode(StatusCodes.Status502BadGateway);
    }
});

app.Run(backendUrl);