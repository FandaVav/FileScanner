using System.Text;
using System.Text.Json;
using FileScannerUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileScannerUI.Controllers
{
    public class ScanController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ScanController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ScanInputViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(ScanInputViewModel model)
        {
            var apiUrl = _config["ApiBaseUrl"] ?? "https://localhost:7289";

            var http = _httpClientFactory.CreateClient();
            var requestBody = new
            {
                directoryPath = model.DirectoryPath,
                includeHidden = model.IncludeHidden,
                includeSubdirectories = model.IncludeSubdirectories
            };

            var json = JsonSerializer.Serialize(requestBody);
            var response = await http.PostAsync($"{apiUrl}/api/scan",
                new StringContent(json, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ScanResultViewModel>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                model.Result = result;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var error = JsonSerializer.Deserialize<ScanErrorResultModel>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    model.ErrorMessage = error?.ErrorMessage ?? "Neznámá chyba.";
                }
                catch
                {
                    model.ErrorMessage = "Nepodařilo se načíst detail chyby.";
                }
            }

            return View(model);
        }
    }
}