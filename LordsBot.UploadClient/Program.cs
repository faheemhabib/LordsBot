using System.Net.Http.Headers;
using System.Text.Json;

namespace LordsBot.UploadClient;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== LordsBot Zip File Upload Client ===\n");

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: LordsBot.UploadClient <path-to-zip-file> [api-url]");
            Console.WriteLine("\nExample:");
            Console.WriteLine("  LordsBot.UploadClient myfile.zip");
            Console.WriteLine("  LordsBot.UploadClient myfile.zip https://localhost:5001");
            return;
        }

        string filePath = args[0];
        string apiUrl = args.Length > 1 ? args[1] : "https://localhost:5001";

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File not found: {filePath}");
            return;
        }

        if (!filePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Error: File must be a ZIP file");
            return;
        }

        var fileInfo = new FileInfo(filePath);
        Console.WriteLine($"File: {fileInfo.Name}");
        Console.WriteLine($"Size: {FormatBytes(fileInfo.Length)}");
        Console.WriteLine($"API URL: {apiUrl}\n");

        await UploadZipFileAsync(filePath, apiUrl);
    }

    static async Task UploadZipFileAsync(string filePath, string apiUrl)
    {
        try
        {
            // Create HTTP client with SSL bypass for localhost (development only)
            using var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
            };
            
            using var client = new HttpClient(httpClientHandler)
            {
                Timeout = TimeSpan.FromMinutes(10) // Long timeout for large files
            };

            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(filePath);
            using var streamContent = new StreamContent(fileStream);
            
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            content.Add(streamContent, "file", Path.GetFileName(filePath));

            Console.WriteLine("Uploading file...");
            var startTime = DateTime.Now;

            var response = await client.PostAsync($"{apiUrl}/api/upload/zip", content);
            
            var duration = DateTime.Now - startTime;
            Console.WriteLine($"Upload completed in {duration.TotalSeconds:F2} seconds\n");

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✓ Upload successful!\n");
                
                var result = JsonSerializer.Deserialize<UploadResult>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result != null)
                {
                    Console.WriteLine($"Message: {result.Message}");
                    Console.WriteLine($"Files extracted: {result.ExtractedFilesCount}");
                    
                    if (result.ExtractedFiles.Count > 0)
                    {
                        Console.WriteLine("\nExtracted files:");
                        foreach (var file in result.ExtractedFiles)
                        {
                            Console.WriteLine($"  - {file}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"✗ Upload failed with status code: {response.StatusCode}\n");
                Console.WriteLine($"Response: {responseContent}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"✗ Connection error: {ex.Message}");
            Console.WriteLine("\nMake sure the API server is running:");
            Console.WriteLine("  cd LordsBot.ZipUpload");
            Console.WriteLine("  dotnet run");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    class UploadResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? FileName { get; set; }
        public long FileSizeBytes { get; set; }
        public int ExtractedFilesCount { get; set; }
        public List<string> ExtractedFiles { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}

