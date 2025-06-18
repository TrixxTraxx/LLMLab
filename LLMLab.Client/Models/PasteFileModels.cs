using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace LLMLab.Client.Models;

// Supporting classes for paste file upload
public class PastedFileInfo
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Type { get; set; } = string.Empty;
    public long LastModified { get; set; }
}

public class RejectedFileInfo
{
    public string Name { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class PastedFileData
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Type { get; set; } = string.Empty;
    public long LastModified { get; set; }
    public string Data { get; set; } = string.Empty; // Base64 encoded file data
}

public class RenamedBrowserFile : IBrowserFile
{
    private readonly IBrowserFile _originalFile;
    private readonly string _newName;

    public RenamedBrowserFile(IBrowserFile originalFile, string newName)
    {
        _originalFile = originalFile;
        _newName = newName;
    }

    public string Name => _newName;
    public DateTimeOffset LastModified => _originalFile.LastModified;
    public long Size => _originalFile.Size;
    public string ContentType => _originalFile.ContentType;

    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
    {
        return _originalFile.OpenReadStream(maxAllowedSize, cancellationToken);
    }
}

public class JSBrowserFile : IBrowserFile
{
    private readonly PastedFileData _fileData;
    private byte[]? _cachedBytes;

    public JSBrowserFile(PastedFileData fileData)
    {
        _fileData = fileData;
    }

    public string Name => _fileData.Name;
    public DateTimeOffset LastModified => DateTimeOffset.FromUnixTimeMilliseconds(_fileData.LastModified);
    public long Size => _fileData.Size;
    public string ContentType => _fileData.Type;

    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
    {
        if (Size > maxAllowedSize)
        {
            throw new IOException($"File size {Size} exceeds maximum allowed size {maxAllowedSize}");
        }

        // Convert base64 data to bytes if not already cached
        if (_cachedBytes == null)
        {
            _cachedBytes = Convert.FromBase64String(_fileData.Data);
        }

        return new MemoryStream(_cachedBytes);
    }
}

public class JSFileStream : Stream
{
    private readonly IJSObjectReference _jsFile;
    private readonly long _length;
    private long _position;
    private bool _disposed;

    public JSFileStream(IJSObjectReference jsFile, long length)
    {
        _jsFile = jsFile;
        _length = length;
        _position = 0;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => _length;
    public override long Position 
    { 
        get => _position; 
        set => throw new NotSupportedException(); 
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(JSFileStream));

        if (_position >= _length)
            return 0;

        var actualCount = (int)Math.Min(count, _length - _position);
        
        try
        {
            // Read data from JavaScript File object
            var arrayBuffer = await _jsFile.InvokeAsync<byte[]>("slice", cancellationToken, _position, _position + actualCount);
            
            if (arrayBuffer != null && arrayBuffer.Length > 0)
            {
                Array.Copy(arrayBuffer, 0, buffer, offset, Math.Min(arrayBuffer.Length, actualCount));
                _position += arrayBuffer.Length;
                return arrayBuffer.Length;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading from JS file stream: {ex.Message}");
        }

        return 0;
    }

    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _jsFile?.DisposeAsync();
            _disposed = true;
        }
        base.Dispose(disposing);
    }
} 