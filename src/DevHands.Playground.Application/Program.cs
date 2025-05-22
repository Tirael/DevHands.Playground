var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability();

await using var app = builder.Build();

app
    .UseObservability()
    .UsePathBase("/api");

app.MapGet("/simulate/cpu/{ms:long}", HandleSimulateCpu);
app.MapGet("/simulate/io/{ms:long}", HandleSimulateIo);
app.MapGet("/hello", HandleHello);

await app.RunAsync();

return;


async Task HandleSimulateCpu(HttpContext context, long ms)
{
    if (ms <= 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Invalid input");
        return;
    }

    var cancellationToken = context.RequestAborted;

    try
    {
        var sw = new Stopwatch();
        sw.Start();

        var data = new byte[1024];

        var endDt = TimeProvider.System.GetUtcNow().AddMilliseconds(ms);

        while (TimeProvider.System.GetUtcNow() < endDt)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (var i = 0; i < 10; i++)
            {
                Random.Shared.NextBytes(data);
                var hash = SHA256.HashData(data);
                NoOpArray(hash);
            }

            Thread.Yield();
        }

        var result = new Payload { TotalTimeMs = sw.ElapsedMilliseconds, Status = "completed" };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsJsonAsync(result);
    }
    catch (OperationCanceledException)
    {
        context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
        await context.Response.WriteAsync("Request timed out");
    }
    catch (Exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("Internal server error");
    }
}

async Task HandleSimulateIo(HttpContext context, long ms)
{
    if (ms <= 0)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Invalid input");
        return;
    }

    var cancellationToken = context.RequestAborted;

    try
    {
        var sw = new Stopwatch();
        sw.Start();

        await Task.Delay(TimeSpan.FromMilliseconds(ms), cancellationToken);

        var result = new Payload { TotalTimeMs = sw.ElapsedMilliseconds, Status = "completed" };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.WriteAsJsonAsync(result);
    }
    catch (OperationCanceledException)
    {
        context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
        await context.Response.WriteAsync("Request timed out");
    }
    catch (Exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("Internal server error");
    }
}

Task HandleHello(HttpContext context)
{
    var helloResult =
        $"""
             <!doctype html>
             <html>
               <head>
                 <title>DevHands.Playground</title>
               </head>
               <body>
                 <h1>Hello DevHands</h1>
                 <p>Current server time {TimeProvider.System.GetUtcNow():O}</p>
               </body>
             </html>
         """;

    context.Response.ContentType = "text/html; charset=utf-8";
    context.Response.StatusCode = StatusCodes.Status200OK;
    return context.Response.WriteAsync(helloResult);
}

[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
void NoOpArray(byte[] hash)
{
    // do nothing
}
