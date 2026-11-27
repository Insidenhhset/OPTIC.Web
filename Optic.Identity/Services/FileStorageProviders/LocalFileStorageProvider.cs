using Optic.Identity.Services.FileStorageProviders;

namespace Optic.Identity.Providers
{
    public class LocalFileStorageProvider : IFileStorageProvider
    {
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public LocalFileStorageProvider(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(
            IFormFile file,
            string folder,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            if (file.Length > 2 * 1024 * 1024)
                throw new InvalidOperationException("File size cannot exceed 2 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("Only JPG, JPEG, PNG and WebP images are allowed.");

            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                folder);

            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";

            var physicalPath = Path.Combine(uploadFolder, fileName);

            await using (var stream = new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            return $"/uploads/{folder}/{fileName}";
        }

        public async Task<byte[]?> ReadFileAsync(
    string filePath,
    CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            var normalizedPath = filePath
                .Replace('\\', '/')
                .TrimStart('/');

            if (!normalizedPath.StartsWith(
                "uploads/",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid file path.");
            }

            var uploadRoot = Path.GetFullPath(
                Path.Combine(_environment.WebRootPath, "uploads"));

            var physicalPath = Path.GetFullPath(
                Path.Combine(_environment.WebRootPath, normalizedPath));

            if (!physicalPath.StartsWith(
                uploadRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid file path.");
            }

            if (!File.Exists(physicalPath))
                return null;

            return await File.ReadAllBytesAsync(
                physicalPath,
                cancellationToken);
        }

        public Task DeleteFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.CompletedTask;

            var normalizedPath = filePath
                .Replace('\\', '/')
                .TrimStart('/');

            var expectedPrefix = "uploads/";

            if (!normalizedPath.StartsWith(
                expectedPrefix,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid upload path.");
            }

            var uploadRoot = Path.GetFullPath(
                Path.Combine(_environment.WebRootPath, "uploads"));

            var physicalPath = Path.GetFullPath(
                Path.Combine(_environment.WebRootPath, normalizedPath));

            if (!physicalPath.StartsWith(
                uploadRoot + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid file path.");
            }

            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            return Task.CompletedTask;
        }
    }
}