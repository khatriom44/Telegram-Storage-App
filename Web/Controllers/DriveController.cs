using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Commands;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers
{
    public class DriveController : Controller
    {
        private readonly IMediator _mediator;

        public DriveController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid? folderId = null)
        {
            var currentFolderId = folderId ?? Guid.Empty;
            var folders = await _mediator.Send(new GetFoldersInParentQuery { ParentFolderId = currentFolderId });
            var files = await _mediator.Send(new GetFilesInFolderQuery { FolderId = currentFolderId });

            var currentFolder = currentFolderId == Guid.Empty
                ? null
                : await _mediator.Send(new GetFolderByIdQuery { FolderId = currentFolderId });

            var viewModel = new DriveViewModel
            {
                CurrentFolderId = currentFolderId,
                CurrentFolderName = currentFolderId == Guid.Empty ? "My Drive" : currentFolder?.Name ?? "Current Folder",
                Folders = folders.Select(f => new FolderViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    CreatedAt = f.CreatedAt
                }).ToList(),
                Files = files.Select(f => new FileViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Size = f.Size,
                    MimeType = f.MimeType,
                    UploadedAt = f.UploadedAt,
                    Icon = GetFileIcon(f.MimeType)
                }).ToList(),
                Breadcrumbs = await GetBreadcrumbs(currentFolderId)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder(string folderName, Guid parentFolderId)
        {
            if (string.IsNullOrWhiteSpace(folderName))
            {
                TempData["Error"] = "Folder name is required.";
                return RedirectToAction(nameof(Index), new { folderId = parentFolderId });
            }
            var command = new CreateFolderCommand
            {
                Name = folderName,
                ParentFolderId = parentFolderId == Guid.Empty ? (Guid?)null : parentFolderId
            };

            await _mediator.Send(command);

            TempData["Success"] = $"Folder '{folderName}' created successfully.";
            return RedirectToAction(nameof(Index), new { folderId = parentFolderId });
        }

        [RequestSizeLimit(2147483647)]
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483647)]
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, Guid folderId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Index), new { folderId });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var command = new UploadFileCommand
                {
                    FileName = file.FileName,
                    FileStream = stream,
                    MimeType = file.ContentType,
                    FolderId = folderId == Guid.Empty ? null : folderId
                };

                await _mediator.Send(command);

                TempData["Success"] = $"File '{file.FileName}' uploaded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Upload failed: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { folderId });
        }

        private string GetFileIcon(string mimeType)
        {
            return mimeType switch
            {
                var t when t.StartsWith("image/") => "image",
                var t when t.StartsWith("video/") => "videocam",
                var t when t.StartsWith("audio/") => "music_note",
                "application/pdf" => "picture_as_pdf",
                var t when t.Contains("word") => "description",
                var t when t.Contains("excel") => "table_chart",
                var t when t.Contains("powerpoint") => "slideshow",
                _ => "insert_drive_file"
            };
        }

        private async Task<List<BreadcrumbItem>> GetBreadcrumbs(Guid folderId)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Id = Guid.Empty, Name = "My Drive" }
            };

            if (folderId == Guid.Empty)
                return breadcrumbs;

            var stack = new Stack<BreadcrumbItem>();
            var currentId = folderId;

            while (currentId != Guid.Empty)
            {
                var folder = await _mediator.Send(new GetFolderByIdQuery { FolderId = currentId });
                if (folder == null)
                    break;

                stack.Push(new BreadcrumbItem { Id = folder.Id, Name = folder.Name });
                currentId = folder.ParentFolderId ?? Guid.Empty;
            }

            while (stack.Count > 0)
            {
                breadcrumbs.Add(stack.Pop());
            }

            return breadcrumbs;
        }
    }
}
