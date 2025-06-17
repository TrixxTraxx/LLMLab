// MinimalFileServer/Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression();

var app = builder.Build();

app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true
});

// Crucial for Blazor WASM routing (SPA fallback)
app.MapFallbackToFile("index.html");

app.Run();