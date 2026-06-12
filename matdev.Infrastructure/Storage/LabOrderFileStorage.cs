using matdev.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace matdev.Infrastructure.Storage;

public class LabOrderFileStorage : ILabOrderFileStorage
{
    private readonly string _rootPath;

    public LabOrderFileStorage(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configured = configuration["FileStorage:RootPath"];
        _rootPath = !string.IsNullOrWhiteSpace(configured) && Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(environment.ContentRootPath, configured ?? "uploads");

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(int projectId, int labOrderId, string reportKind, Stream content, string fileName)
    {
        var safeName = SanitizeFileName(fileName);
        var relative = $"{projectId}/{labOrderId}/{reportKind}/{Guid.NewGuid():N}-{safeName}";
        var fullPath = Path.Combine(_rootPath, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs);

        return ILabOrderFileStorage.ToStoredLink(relative);
    }

    public (Stream Stream, string FileName, string ContentType)? OpenRead(string? storedLink, string? displayFileName)
    {
        if (!ILabOrderFileStorage.IsStoredLink(storedLink))
            return null;

        var fullPath = Path.Combine(_rootPath, ILabOrderFileStorage.FromStoredLink(storedLink!));
        if (!File.Exists(fullPath))
            return null;

        var fileName = string.IsNullOrWhiteSpace(displayFileName)
            ? Path.GetFileName(fullPath)
            : displayFileName;

        var stream = File.OpenRead(fullPath);
        return (stream, fileName, GetContentType(fileName));
    }

    public void DeleteIfExists(string? storedLink)
    {
        if (!ILabOrderFileStorage.IsStoredLink(storedLink))
            return;

        var fullPath = Path.Combine(_rootPath, ILabOrderFileStorage.FromStoredLink(storedLink!));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    public void DeleteOrderFolder(int projectId, int labOrderId)
    {
        var folder = Path.Combine(_rootPath, projectId.ToString(), labOrderId.ToString());
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    private static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return string.IsNullOrWhiteSpace(name) ? "report.bin" : name;
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".csv" => "text/csv",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream",
        };
    }
}
