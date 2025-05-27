using System.Text.Json;

namespace FileScanner.Storage
{
    public class MasterLibrary
    {
        private static readonly string LibraryDir = Path.Combine(AppContext.BaseDirectory, "LibraryRecords");
        private static readonly string IndexFile = Path.Combine(LibraryDir, "MasterLibrary.json");

        private Dictionary<string, string> _pathToFileMap = new(StringComparer.OrdinalIgnoreCase);

        public MasterLibrary()
        {
            if (File.Exists(IndexFile))
            {
                var json = File.ReadAllText(IndexFile);
                _pathToFileMap = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            if (!Directory.Exists(LibraryDir))
                Directory.CreateDirectory(LibraryDir);
        }

        public string GetSnapshotFile(string rawPath)
        {
            var normalized = NormalizePath(rawPath);

            if (_pathToFileMap.TryGetValue(normalized, out var filename))
            {
                return Path.Combine(LibraryDir, filename);
            }

            
            filename = GenerateFileNameFromPath(normalized);
            int counter = 1;

            while (_pathToFileMap.Values.Contains(filename))
            {
                filename = GenerateFileNameFromPath(normalized, counter++);
            }

            _pathToFileMap[normalized] = filename;
            Save();

            return filename;
        }

        private void Save()
        {
            var json = JsonSerializer.Serialize(_pathToFileMap, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(IndexFile, json);
        }

        public static string NormalizePath(string path)
        {
            var full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            return full.ToLowerInvariant();
        }

        private static string GenerateFileNameFromPath(string normalizedPath, int? suffix = null)
        {
            var driveLetter = normalizedPath.Length >= 2 && normalizedPath[1] == ':' ? normalizedPath[0].ToString() : "drive";
            var rest = normalizedPath.Substring(2).Replace(Path.DirectorySeparatorChar, '-').Replace('/', '-');
            var safe = $"scan-{driveLetter}-{rest}".Replace("--", "-");

            if (suffix.HasValue)
                safe += $"-{suffix.Value}";

            return safe + ".json";
        }
    }
}
