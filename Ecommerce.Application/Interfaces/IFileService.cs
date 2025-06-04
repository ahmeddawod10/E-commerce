using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Application.Interfaces
{
    public interface IFileService
    {

        /// <summary>
        /// Saves a file asynchronously to the specified upload folder and returns its relative URL path.
        /// </summary>
        /// <param name="file">The IFormFile to save.</param>
        /// <param name="allowedExtensions">An array of allowed file extensions (e.g., {".jpg", ".png"}).</param>
        /// <param name="maxFileSizeMb">Optional: Maximum allowed file size in megabytes. Defaults to 5MB.</param>
        /// <returns>The relative URL path to the saved file (e.g., "/uploads/uniqueid.png").</returns>
        /// <exception cref="ArgumentNullException">Thrown if file is null.</exception>
        /// <exception cref="ArgumentException">Thrown if file is empty, has an invalid extension, or exceeds max size.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there's an error during file saving (e.g., I/O error).</exception>
        Task<string> SaveFileAsync(IFormFile file, string[] allowedExtensions, int maxFileSizeMb = 5);

        /// <summary>
        /// Deletes a file from the server.
        /// </summary>
        /// <param name="relativeFilePath">The relative URL path to the file (e.g., "/uploads/uniqueid.png").</param>
        /// <returns>True if the file was deleted or did not exist, false if deletion failed for other reasons.</returns>
        /// <exception cref="InvalidOperationException">Thrown if there's an error during file deletion (e.g., I/O error).</exception>
        bool DeleteFile(string relativeFilePath);
    }

   
}