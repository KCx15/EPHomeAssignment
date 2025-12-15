using HomeAssignment.Factories;
using HomeAssignment.Repositories;
using HomeAssignment.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

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
            return View();
        }

        // ---------------------------------------------------------
        // STEP 1: Upload JSON → Parse → Store in MEMORY (NOT DB)
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> BulkImport(
            IFormFile jsonFile,
            [FromKeyedServices("memory")] IItemsRepository memoryRepo)
        {
            if (jsonFile == null || jsonFile.Length == 0)
                return BadRequest("No JSON file uploaded.");

            string json;
            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
                json = await reader.ReadToEndAsync();

            // Parse items from JSON using the updated factory
            var items = _factory.Create(json);

            // Store in memory BEFORE approval
            await memoryRepo.SaveAsync(items);

            // Show preview screen
            return View("Preview", items);
        }

        // ---------------------------------------------------------
        // STEP 2: User clicks “Commit to Database”
        // Save Restaurants → Remap IDs → Save MenuItems
   

[HttpPost]
    public async Task<IActionResult> CommitToDatabase(
    [FromKeyedServices("memory")] IItemsRepository memoryRepo,
    [FromKeyedServices("db")] IItemsRepository dbRepo,
    IFormFile? imagesZip)
    {
        var items = (await memoryRepo.GetAsync()).ToList();
        if (!items.Any())
            return BadRequest("No imported items found. Please upload JSON first.");

        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(wwwroot, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var defaultRelative = "/images/default.jpg";
        var defaultImagePath = Path.Combine(wwwroot, "images", "default.jpg");

        ZipArchive? zip = null;
        List<string> entryNames = new();

        if (imagesZip != null && imagesZip.Length > 0)
        {
            zip = new ZipArchive(imagesZip.OpenReadStream(), ZipArchiveMode.Read);

            // Collect entry names for debugging (we’ll use if matching fails)
            entryNames = zip.Entries.Select(e => e.FullName).ToList();
        }

        static string Norm(string p) => (p ?? "")
            .Replace('\\', '/')
            .TrimStart('/')
            .ToLowerInvariant();

        bool anyZipImageMatched = false;

        for (int index = 1; index <= items.Count; index++)
        {
            var item = items[index - 1];
            string? imageUrl = null;

            if (zip != null)
            {
                // We want ANY image file inside folder "item-{index}" (any depth)
                var folderToken = $"/item-{index}/";
                var folderToken2 = $"item-{index}/";

                var entry = zip.Entries.FirstOrDefault(e =>
                {
                    var n = Norm(e.FullName);

                    // must contain item-x as a folder
                    bool inFolder = n.Contains(folderToken) || n.StartsWith(folderToken2);

                    // must be a file (not folder)
                    bool isFile = !n.EndsWith("/");

                    // must be an image extension
                    var ext = Path.GetExtension(n);
                    bool isImage = ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp";

                    return inFolder && isFile && isImage;
                });

                if (entry != null)
                {
                    anyZipImageMatched = true;

                    var ext = Path.GetExtension(entry.Name);
                    if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";

                    var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(filePath, FileMode.Create);
                    await entryStream.CopyToAsync(fileStream);

                    imageUrl = $"/uploads/{uniqueFileName}";
                }
            }

            // fallback
            if (string.IsNullOrEmpty(imageUrl) && System.IO.File.Exists(defaultImagePath))
                imageUrl = defaultRelative;

            if (item is Restaurant r) r.ImageUrl = imageUrl;
            else if (item is MenuItem m) m.ImageUrl = imageUrl;
        }

        // 🔥 DEBUG: If ZIP was uploaded but we matched ZERO images, show what's inside the ZIP
        if (zip != null && !anyZipImageMatched)
        {
            var sample = entryNames.Take(50);
            return BadRequest(
                "Uploaded ZIP received, but no images were found for item-x folders. " +
                "ZIP entries (first 50):\n" + string.Join("\n", sample)
            );
        }

        await dbRepo.SaveAsync(items);

        return RedirectToAction("Index", "Verification");
    }




    // ---------------------------------------------------------
    // STEP 3: (Assignment AA4.3) Upload ZIP of images
    // Placeholder — You will implement next
    // ---------------------------------------------------------
    [HttpPost]
        public async Task<IActionResult> UploadImages(IFormFile imagesZip)
        {
            if (imagesZip == null || imagesZip.Length == 0)
                return BadRequest("No ZIP uploaded");

            // TODO: Extract ZIP → Save images → Map to item IDs → Store URL in DB

            return Ok("ZIP upload handler placeholder (AA4.3)");
        }

        [HttpPost]
        public async Task<IActionResult> DownloadImagesZip(
    [FromKeyedServices("memory")] IItemsRepository memoryRepo)
        {
            var items = await memoryRepo.GetAsync();
            var list = items.ToList();

            if (!list.Any())
                return BadRequest("No items in memory. Import JSON first.");

            // Path to default image
            var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "default.jpg");
            if (!System.IO.File.Exists(defaultImagePath))
                return NotFound("Default image not found at wwwroot/images/default.jpg");

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                int index = 1;
                foreach (var item in list)
                {
                    var folderName = $"item-{index}";
                    var entryPath = $"{folderName}/default.jpg";

                    var entry = zip.CreateEntry(entryPath, CompressionLevel.Fastest);
                    using var entryStream = entry.Open();
                    using var fileStream = System.IO.File.OpenRead(defaultImagePath);
                    fileStream.CopyTo(entryStream);

                    index++;
                }
            }

            ms.Position = 0;
            return File(ms.ToArray(), "application/zip", "images-template.zip");
        }

    }
}
