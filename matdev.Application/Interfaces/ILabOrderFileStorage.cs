namespace matdev.Application.Interfaces;

public interface ILabOrderFileStorage
{
    const string StoredLinkPrefix = "file:";

    static bool IsStoredLink(string? link) =>
        link?.StartsWith(StoredLinkPrefix, StringComparison.OrdinalIgnoreCase) == true;

    static string ToStoredLink(string relativePath) => StoredLinkPrefix + relativePath;

    static string FromStoredLink(string storedLink) =>
        storedLink.StartsWith(StoredLinkPrefix, StringComparison.OrdinalIgnoreCase)
            ? storedLink[StoredLinkPrefix.Length..]
            : storedLink;

    Task<string> SaveAsync(int projectId, int labOrderId, string reportKind, Stream content, string fileName);

    (Stream Stream, string FileName, string ContentType)? OpenRead(string? storedLink, string? displayFileName);

    void DeleteIfExists(string? storedLink);

    void DeleteOrderFolder(int projectId, int labOrderId);
}
