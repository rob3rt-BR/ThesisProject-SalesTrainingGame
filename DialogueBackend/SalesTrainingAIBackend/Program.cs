using DialogueModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<OpenRouterService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1/");
    client.Timeout = TimeSpan.FromSeconds(45);
});

var app = builder.Build();

string backendUrl = "http://localhost:5065";

app.MapPost("/api/dialogue", async (DialogueRequest request, OpenRouterService openRouterService, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await openRouterService.GenerateDialogueAsync(request, cancellationToken);

        return Results.Ok(result);
    }
    catch (OpenRouterRateLimitException ex)
    {
        return Results.Problem(
            title: "OpenROuter rate limit reached",
            detail: ex.Message,
            statusCode: StatusCodes.Status429TooManyRequests
        );
    }
    catch (InvalidDialogueFormatException ex)
    {
        return Results.Problem(
            title: "Invalid AI response",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway
        );
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            title: "OpenROuter request failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway
        );
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            title: "Backend configuration error",
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError
        );
    } 
});

app.Run(backendUrl);

