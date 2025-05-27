namespace PracticeTask.Models
{
    public class ScanResult
    {
        public List<FileChange> Added { get; set; } = new();
        public List<FileChange> Changed { get; set; } = new();
        public List<FileChange> RemovedFiles { get; set; } = new();
        public List<string> RemovedDirectories { get; set; } = new();
        
    }
}
