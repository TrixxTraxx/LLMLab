using LLMLab.Client.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace LLMLab.Client.Services;

public class FileUtilityService
{
    /// <summary>
    /// Generates a unique filename by appending a number in parentheses if duplicates exist
    /// </summary>
    /// <param name="originalFileName">The original filename to check</param>
    /// <param name="existingFileNames">Collection of existing filenames to check against</param>
    /// <returns>A unique filename</returns>
    public string GenerateUniqueFileName(string originalFileName, IEnumerable<string> existingFileNames)
    {
        var existingNames = existingFileNames.ToHashSet();
        
        if (!existingNames.Contains(originalFileName))
        {
            return originalFileName;
        }
        
        // Split filename and extension
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);
        
        var counter = 1;
        string uniqueName;
        
        do
        {
            uniqueName = $"{nameWithoutExtension} ({counter}){extension}";
            counter++;
        } while (existingNames.Contains(uniqueName));
        
        return uniqueName;
    }

    /// <summary>
    /// Creates a wrapped IBrowserFile with a new filename while preserving all other properties
    /// </summary>
    /// <param name="originalFile">The original file to wrap</param>
    /// <param name="newFileName">The new filename to use</param>
    /// <returns>IBrowserFile with the new filename</returns>
    public IBrowserFile CreateRenamedFile(IBrowserFile originalFile, string newFileName)
    {
        return new RenamedBrowserFile(originalFile, newFileName);
    }

    /// <summary>
    /// Validates if a file type is supported by checking content type against supported types
    /// </summary>
    /// <param name="contentType">The file's content type</param>
    /// <param name="supportedContentTypes">Comma-separated list of supported content types</param>
    /// <returns>True if the file type is supported</returns>
    public bool IsFileTypeSupported(string contentType, string? supportedContentTypes)
    {
        if (string.IsNullOrEmpty(supportedContentTypes))
        {
            // If no supported content types are defined, allow all file types
            return true;
        }

        // Parse supported content types (assuming comma-separated list)
        var supportedTypes = supportedContentTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .ToList();

        // Check if content type matches any of the supported types
        var normalizedContentType = contentType.ToLowerInvariant();
        
        return supportedTypes.Any(supportedType =>
        {
            // Support wildcard patterns like "image/*" or exact matches
            if (supportedType.EndsWith("/*"))
            {
                var prefix = supportedType.Substring(0, supportedType.Length - 2);
                return normalizedContentType.StartsWith(prefix + "/");
            }
            return normalizedContentType == supportedType;
        });
    }

    /// <summary>
    /// Checks if a file is an image based on its content type
    /// </summary>
    /// <param name="contentType">The file's content type</param>
    /// <returns>True if the file is an image</returns>
    public bool IsImageFile(string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates file size against a maximum allowed size
    /// </summary>
    /// <param name="fileSize">The file size in bytes</param>
    /// <param name="maxSizeInBytes">Maximum allowed size in bytes</param>
    /// <returns>True if the file size is within limits</returns>
    public bool IsFileSizeValid(long fileSize, long maxSizeInBytes)
    {
        return fileSize <= maxSizeInBytes;
    }

    /// <summary>
    /// Formats file size for display (e.g., "1.5 MB", "256 KB")
    /// </summary>
    /// <param name="bytes">File size in bytes</param>
    /// <returns>Formatted file size string</returns>
    public string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
} 