namespace FileScannerUI.Models
{
    public class ScanInputViewModel
    {
        public string DirectoryPath { get; set; }
        public bool IncludeHidden { get; set; }
        public bool IncludeSubdirectories { get; set; }

       
        public ScanResultViewModel? Result { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
