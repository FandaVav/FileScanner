using PracticeTask.Models;

namespace PracticeTask.Services
{
    public interface IDirectoryScanner
    {
        Task<ScanResult> ScanAsync(ScanRequest request);
    }
}
