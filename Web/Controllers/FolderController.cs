using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class FolderController : Controller
    {
        private readonly ITelegramService _telegramService;
        private readonly ILogger<FolderController> _logger;

        public FolderController(ITelegramService telegramService, ILogger<FolderController> logger)
        {
            _telegramService = telegramService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var folders = await _telegramService.GetAllFoldersAsync();
                var folderDtos = folders.Select(f => new FolderDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                }).OrderByDescending(f => f.CreatedAt).ToList();

                return View(folderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading folders");
                TempData["Error"] = "Failed to load folders. Please try again.";
                return View(new List<FolderDto>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] FolderDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid folder name" });
                }

                var folder = new Folder
                {
                    Id = Guid.NewGuid(),
                    Name = model.Name.Trim()
                };

                await _telegramService.CreateFolderAsync(folder);

                return Json(new
                {
                    success = true,
                    folder = new FolderDto
                    {
                        Id = folder.Id,
                        Name = folder.Name,
                        CreatedAt = folder.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder");
                return Json(new { success = false, message = "Failed to create folder" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RenameFolder([FromBody] FolderDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Invalid folder name" });
                }

                var existingFolder = await _telegramService.GetFolderByIdAsync(model.Id);
                if (existingFolder == null)
                {
                    return Json(new { success = false, message = "Folder not found" });
                }

                existingFolder.Name = model.Name.Trim();
                await _telegramService.UpdateFolderAsync(existingFolder);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renaming folder");
                return Json(new { success = false, message = "Failed to rename folder" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFolder([FromBody] DeleteFolderRequest request)
        {
            try
            {
                await _telegramService.DeleteFolderAsync(request.Id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting folder");
                return Json(new { success = false, message = "Failed to delete folder" });
            }
        }
        public async Task<IActionResult> Details(Guid id)
        {
            var folder = await _telegramService.GetFolderByIdAsync(id);
            if (folder == null) return NotFound();

            // Fetch files in folder (implement in service)
            var files = await _telegramService.GetFilesInFolderAsync(id);

            var vm = new FolderDetailsViewModel
            {
                Folder = folder,
                Files = files
            };
            return View(vm);
        }

    }

    public class DeleteFolderRequest
    {
        public Guid Id { get; set; }
    }
}

