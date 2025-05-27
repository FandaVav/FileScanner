using System.Security.Cryptography;
using System.Text.Json;
using FileScanner.Storage;
using PracticeTask.Models;
using PracticeTask.Services;

namespace PracticeTask.Services;

public class DirectoryScanner : IDirectoryScanner
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private readonly ILogger<DirectoryScanner> _logger;

    public DirectoryScanner(ILogger<DirectoryScanner> logger)
    {
        _logger = logger;
    }

    public async Task<ScanResult> ScanAsync(ScanRequest request)
    {
        // používám zámek aby dvě provolání z různých klientů neproběhli pravidelně a neudělaly nepořádek ve verzování. 
        await _lock.WaitAsync();
        try
        {
            var library = new MasterLibrary();
            var snapshotFile = library.GetSnapshotFile(request.DirectoryPath);

            var previous = await LoadCacheAsync(snapshotFile);
            var current = await CreateSnapshotAsync(request);
            var result = CompareSnapshots(previous, current, request);
            await SaveCacheAsync(snapshotFile, current);
            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<CacheFile> LoadCacheAsync(string path)
    {

        if (!File.Exists(path))
            return new CacheFile();

        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<CacheFile>(json) ?? new CacheFile();
    }

    private async Task SaveCacheAsync(string path, CacheFile snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
    }

    private async Task<CacheFile> CreateSnapshotAsync(ScanRequest request)
    {
        var searchOption = request.IncludeSubdirectories
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        var allFiles = Directory.GetFiles(request.DirectoryPath, "*", searchOption);
        var allDirs = Directory.GetDirectories(request.DirectoryPath, "*", searchOption);

        var files = new Dictionary<string, FileSnapshot>();
        var folders = new HashSet<string>();

        foreach (var file in allFiles)
        {
            var isHidden = IsHidden(file);
            if (!request.IncludeHidden && IsHidden(file))
                continue;

            var relPath = Path.GetRelativePath(request.DirectoryPath, file).Replace('\\', '/');
            var hash = await ComputeFileHashAsync(file);

            files[relPath] = new FileSnapshot
            {
                Hash = hash,
                Version = 1,
                IsHidden = isHidden
            };

            
            var dir = Path.GetDirectoryName(relPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(dir))
                folders.Add(dir);
        }

        
        foreach (var dirPath in allDirs)
        {
            if (!request.IncludeHidden && IsHidden(dirPath))
                continue;

            var relPath = Path.GetRelativePath(request.DirectoryPath, dirPath).Replace('\\', '/');
            if (!string.IsNullOrEmpty(relPath))
                folders.Add(relPath);
        }

        folders.Add(""); 

        return new CacheFile
        {
            Files = files,
            Folders = folders.ToList()
        };
    }
    private ScanResult CompareSnapshots(CacheFile previous, CacheFile current, ScanRequest request)
    {
        var added = new List<FileChange>();
        var changed = new List<FileChange>();
        var removed = new List<FileChange>();
        var removedDirs = new List<string>();

        var updatedFiles = new Dictionary<string, FileSnapshot>();

        foreach (var (relPath, file) in current.Files)
        {
            if (previous.Files.TryGetValue(relPath, out var old))
            {
                if (file.Hash != old.Hash)
                {
                    file.Version = old.Version + 1;
                    changed.Add(new FileChange { RelativePath = relPath, Version = file.Version });
                }
                else
                {
                    file.Version = old.Version;
                }
            }
            else
            {
                file.Version = 1;
                added.Add(new FileChange { RelativePath = relPath, Version = 1 });
            }

            updatedFiles[relPath] = file;
        }

        foreach (var (relPath, oldFile) in previous.Files)
        {
            var shouldInclude = oldFile.IsHidden ? request.IncludeHidden : true;

            if (!shouldInclude)
                continue;

            if (!current.Files.ContainsKey(relPath))
            {
                removed.Add(new FileChange { RelativePath = relPath, Version = oldFile.Version });
            }
        }

        foreach (var oldDir in previous.Folders)
        {
            if (!current.Folders.Contains(oldDir))
            {
                removedDirs.Add(oldDir);
            }
        }

       
        var finalFiles = updatedFiles;

        
        foreach (var (relPath, oldFile) in previous.Files)
        {
            var shouldInclude = oldFile.IsHidden ? request.IncludeHidden : true;

            if (!shouldInclude && !finalFiles.ContainsKey(relPath))
            {
                finalFiles[relPath] = oldFile;
            }
        }

        
        current.Files = finalFiles;

        return new ScanResult
        {
            Added = added,
            Changed = changed,
            RemovedFiles = removed,
            RemovedDirectories = removedDirs
        };
    }


    private async Task<string> ComputeFileHashAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private static bool IsHidden(string path)
    {
        var attributes = File.GetAttributes(path);
        var name = Path.GetFileName(path);
        return (attributes.HasFlag(FileAttributes.Hidden)) || name.StartsWith(".");
    }
}