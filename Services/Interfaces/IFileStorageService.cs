namespace GestionProduccion.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder);
    Task DeleteAsync(string fileName, string folder);
    string GetUrl(string fileName, string folder);
}
