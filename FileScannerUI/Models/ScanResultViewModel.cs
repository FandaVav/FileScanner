namespace FileScannerUI.Models
{
    public class ScanResultViewModel
    {
        public List<FileChangeViewModel> Added { get; set; } = new();
        public List<FileChangeViewModel> Changed { get; set; } = new();
        public List<FileChangeViewModel> RemovedFiles { get; set; } = new();
        public List<string> RemovedDirectories { get; set; } = new();
    }

    public class FileChangeViewModel
    {
        public string RelativePath { get; set; }
        public int Version { get; set; }
    }
}
