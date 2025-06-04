using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;  

namespace Ecommerce.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileService> _logger;
        private const string UploadsFolderName = "uploads";  

        public FileService(IWebHostEnvironment webHostEnvironment, ILogger<FileService> logger)
        {
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> SaveFileAsync(IFormFile file, string[] allowedExtensions, int maxFileSizeMb = 5)
        {
            if (file == null)
            {
                _logger.LogWarning("Attempted to save a null file.");
                throw new ArgumentNullException(nameof(file), "File cannot be null.");
            }

            if (file.Length == 0)
            {
                _logger.LogWarning("Attempted to save an empty file.");
                throw new ArgumentException("File is empty.", nameof(file));
            }

             
            long maxFileSizeBytes = (long)maxFileSizeMb * 1024 * 1024;
            if (file.Length > maxFileSizeBytes)
            {
                _logger.LogWarning($"File size {file.Length} bytes exceeds maximum allowed {maxFileSizeBytes} bytes.");
                throw new ArgumentException($"File size exceeds {maxFileSizeMb} MB.", nameof(file));
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (allowedExtensions == null || !allowedExtensions.Any())
            {
                _logger.LogWarning("No allowed file extensions specified. All extensions will be allowed.");
            }
            else if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning($"File extension '{fileExtension}' not allowed for file '{file.FileName}'.");
                throw new ArgumentException($"File type '{fileExtension}' is not allowed. Allowed types are: {string.Join(", ", allowedExtensions)}", nameof(file));
            }

            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;

            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, UploadsFolderName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
                _logger.LogInformation($"Created upload directory: {uploadPath}");
            }

            var filePath = Path.Combine(uploadPath, uniqueFileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/{UploadsFolderName}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save file '{file.FileName}' to '{filePath}'.");
                throw new InvalidOperationException("Failed to save file.", ex);
            }
        }

        public bool DeleteFile(string relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath))
            {
                _logger.LogWarning("Attempted to delete a file with a null or empty relative path.");
                return false;  
            }

            var normalizedPath = relativeFilePath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, normalizedPath);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Successfully deleted file: {filePath}");
                    return true;
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, $"I/O error deleting file: {filePath}");
                    throw new InvalidOperationException($"Could not delete file due to I/O error: {ex.Message}", ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, $"Unauthorized access attempting to delete file: {filePath}");
                    throw new UnauthorizedAccessException($"Access denied to delete file: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An unexpected error occurred while deleting file: {filePath}");
                    throw new InvalidOperationException($"An unexpected error occurred during file deletion: {ex.Message}", ex);
                }
            }
            else
            {
                _logger.LogWarning($"Attempted to delete non-existent file: {filePath}");
                return false; 
            }
        }
    }

}
