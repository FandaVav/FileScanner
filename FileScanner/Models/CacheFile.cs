namespace PracticeTask.Models
{

    public class CacheFile
    {
        public Dictionary<string, FileSnapshot> Files { get; set; } = new();
        public List<string> Folders { get; set; } = new();
    }
}
