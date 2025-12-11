using HomeAssignment.Factories;
using HomeAssignment.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace HomeAssignment.Controllers
{
    public class BulkImportController : Controller
    {
        private readonly ImportItemFactory _factory;

        public BulkImportController(ImportItemFactory factory)
        {
            _factory = factory;
        }

        // GET: /BulkImport
        [HttpGet]
        public IActionResult Index()
        {
            return View(); // This will look for Views/BulkImport/Index.cshtml
        }

        // POST: /BulkImport
        [HttpPost]
        public async Task<IActionResult> BulkImport(
            IFormFile jsonFile,
            [FromKeyedServices("memory")] IItemsRepository memoryRepo)
        {
            if (jsonFile == null || jsonFile.Length == 0)
                return BadRequest("No file uploaded");

            string json;
            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                json = await reader.ReadToEndAsync();
            }

            var items = _factory.Create(json);

            // Store temporarily (NOT in DB)
            await memoryRepo.SaveAsync(items);

            // Show preview
            return View("Preview", items);
        }
    }
}
