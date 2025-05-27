using FileScanner.Models;
using Microsoft.AspNetCore.Mvc;
using PracticeTask.Models;
using PracticeTask.Services;
using System.Text.Json;

namespace PracticeTask.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScanController : ControllerBase
    {
        private readonly IDirectoryScanner _scanner;
        private readonly ILogger<ScanController> _logger;

        public ScanController(IDirectoryScanner scanner, ILogger<ScanController> logger)
        {
            _scanner = scanner;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ScanRequest request)
        {
            try
            {
                var result = await _scanner.ScanAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba při zpracování požadavku ScanAsync");

                return StatusCode(500, new ScanErrorResult
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost("admin/view-master-library")]
        public IActionResult ViewMasterLibrary()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "LibraryRecords", "MasterLibrary.json");

                if (!System.IO.File.Exists(path))
                    return NotFound("MasterLibrary.json not found.");

                var json = System.IO.File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba při načítání MasterLibrary");

                return StatusCode(500, new ScanErrorResult
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpPost("admin/view-snapshot-by-name")]
        public async Task<IActionResult> ViewSnapshotByName([FromBody] string fileName)
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "LibraryRecords", fileName);

                if (!System.IO.File.Exists(path))
                    return NotFound($"Snapshot file '{fileName}' not found.");

                var json = await System.IO.File.ReadAllTextAsync(path);
                var snapshot = JsonSerializer.Deserialize<CacheFile>(json);
                return Ok(snapshot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Chyba při načítání snapshotu {FileName}", fileName);

                return StatusCode(500, new ScanErrorResult
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}