using GestionProduccion.Services.Interfaces;

namespace GestionProduccion.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", folder);
        if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    public Task DeleteAsync(string fileName, string folder)
    {
        var filePath = Path.Combine(_env.WebRootPath, "uploads", folder, fileName);
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.CompletedTask;
    }

    public string GetUrl(string fileName, string folder)
    {
        return $"/uploads/{folder}/{fileName}";
    }
}
