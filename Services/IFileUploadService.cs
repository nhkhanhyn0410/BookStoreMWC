// Services/IFileUploadService.cs
using Microsoft.AspNetCore.Http;

namespace BookStoreMVC.Services
{
    public interface IFileUploadService
    {
        Task<FileUploadResult> UploadImageAsync(IFormFile file, string folder = "books");
        Task<bool> DeleteImageAsync(string imageUrl);
        bool IsValidImageFile(IFormFile file);
        string GetImagePath(string fileName, string folder = "books");
        Task<FileUploadResult> ResizeAndUploadImageAsync(IFormFile file, string folder = "books", int maxWidth = 800, int maxHeight = 1200);
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string? ImageUrl { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedContentTypes = {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"
        };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadImageAsync(IFormFile file, string folder = "books")
        {
            var result = new FileUploadResult();

            try
            {
                // Validate file
                if (!IsValidImageFile(file))
                {
                    result.ErrorMessage = "Invalid image file. Please upload JPG, PNG, GIF, or WebP files only.";
                    return result;
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";

                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, "images", folder);
                Directory.CreateDirectory(uploadPath);

                // Full file path
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return success result
                result.Success = true;
                result.ImageUrl = $"/images/{folder}/{fileName}";
                result.FileName = fileName;
                result.ContentType = file.ContentType;
                result.FileSize = file.Length;

                _logger.LogInformation("Image uploaded successfully: {FileName}", fileName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {FileName}", file.FileName);
                result.ErrorMessage = "An error occurred while uploading the image.";
                return result;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("http"))
                    return true; // External URL or empty, nothing to delete

                var relativePath = imageUrl.TrimStart('/');
                var filePath = Path.Combine(_environment.WebRootPath, relativePath);

                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation("Image deleted successfully: {FilePath}", filePath);
                    return true;
                }

                return true; // File doesn't exist, consider it deleted
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
                return false;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size
            if (file.Length > MaxFileSize)
                return false;

            // Check content type
            if (!_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            return true;
        }

        public string GetImagePath(string fileName, string folder = "books")
        {
            return $"/images/{folder}/{fileName}";
        }

        public async Task<FileUploadResult> ResizeAndUploadImageAsync(IFormFile file, string folder = "books", int maxWidth = 800, int maxHeight = 1200)
        {
            // For now, just upload without resizing
            // In a real application, you would use a library like ImageSharp to resize images
            return await UploadImageAsync(file, folder);
        }
    }
}