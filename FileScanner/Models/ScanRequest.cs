namespace PracticeTask.Models
{
    public class ScanRequest
    {
        public string DirectoryPath { get; set; }
        public bool IncludeHidden { get; set; }
        public bool IncludeSubdirectories { get; set; }
    }
}
