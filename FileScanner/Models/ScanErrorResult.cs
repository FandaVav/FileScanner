namespace FileScanner.Models
{
    public class ScanErrorResult
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
    }
}
