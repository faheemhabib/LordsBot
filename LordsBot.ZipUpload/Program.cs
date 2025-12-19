using LordsBot.ZipUpload.Services;
using LordsBot.ZipUpload.Models;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ZipFileService>();

// Configure form options for large file uploads (160MB+)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200 * 1024 * 1024; // 200MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Configure Kestrel server limits
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 200 * 1024 * 1024; // 200MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// API endpoint to upload and process zip files
app.MapPost("/api/upload/zip", async (IFormFile file, ZipFileService zipService) =>
{
    var result = await zipService.ProcessZipFileAsync(file);
    
    if (result.Success)
    {
        return Results.Ok(result);
    }
    
    return Results.BadRequest(result);
})
.WithName("UploadZipFile")
.WithOpenApi()
.DisableAntiforgery()
.Accepts<IFormFile>("multipart/form-data")
.Produces<UploadResult>(200)
.Produces<UploadResult>(400);

// Health check endpoint
app.MapGet("/api/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    maxFileSizeMB = 200 
}))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
