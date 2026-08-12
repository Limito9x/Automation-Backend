namespace Automation.Files.Domain.Entities;

public class Asset : BaseEntity<Guid>
{
    public string StoragePath { get; private set; } = string.Empty;

    public long SizeBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public string Extension { get; private set; } = string.Empty;
    
    // Hash SHA-256 to deduplicate files
    public string HashSha256 { get; private set; } = string.Empty;
    
    // Check if this file is confirmed to be on R2
    public bool IsConfirmed { get; private set; }

    protected Asset() { } // EF Core

    public Asset(string storagePath, long sizeBytes, string contentType, string extension, string hashSha256)
    {
        Id = Guid.NewGuid();
        StoragePath = storagePath;
        SizeBytes = sizeBytes;
        ContentType = contentType;
        Extension = extension;
        HashSha256 = hashSha256;
        IsConfirmed = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsConfirmed()
    {
        IsConfirmed = true;
    }
}



