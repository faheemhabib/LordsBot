namespace LordsBot.ZipUpload.Models;

/// <summary>
/// Information about a file extracted from the zip
/// </summary>
public class ZipFileInfo
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string FileType { get; set; } = string.Empty;
    public bool IsExecutable { get; set; }
}
