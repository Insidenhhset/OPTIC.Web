namespace Optic.Identity.Services.FileStorageProviders
{
    public interface IFileStorageProvider
    {
        Task<string> SaveFileAsync(
            IFormFile file,
            string folder,
            CancellationToken cancellationToken = default);

        Task DeleteFileAsync(
            string filePath,
            CancellationToken cancellationToken = default);

        Task<byte[]?> ReadFileAsync(
            string filePath,
            CancellationToken cancellationToken = default);
    }
}
