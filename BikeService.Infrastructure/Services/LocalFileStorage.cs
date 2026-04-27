using BikeService.Application.Interfaces.FileStorage;
using Microsoft.AspNetCore.Hosting;

namespace BikeService.Infrastructure.Services;

public class LocalFileStorage : IFileStorage
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadFileAsync(Stream content, string fileName, string folder)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(uploadsPath);

        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadsPath, uniqueName);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await content.CopyToAsync(fileStream);

        return $"/uploads/{folder}/{uniqueName}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }
}
