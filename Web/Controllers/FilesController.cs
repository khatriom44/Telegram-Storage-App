using Application.Commands;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{

        [ApiController]
        [Route("api/[controller]")]
        public class FilesController : Controller
        {
            private readonly IMediator _mediator;

            public FilesController(IMediator mediator)
            {
                _mediator = mediator;
            }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [RequestSizeLimit(2147483647)] // ~2GB
        [RequestFormLimits(MultipartBodyLengthLimit = 2147483647)]
        [HttpPost("upload")]
            public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] Guid folderId)
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded");

                try
                {
                    using var stream = file.OpenReadStream();
                    var command = new UploadFileCommand
                    {
                        FileName = file.FileName,
                        FileStream = stream,
                        MimeType = file.ContentType,
                        FolderId = folderId
                    };

                    var fileId = await _mediator.Send(command);
                    return Ok(new { fileId, message = "File uploaded successfully" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Upload failed: {ex.Message}");
                }
            }

            [HttpGet("folder/{folderId}")]
            public async Task<IActionResult> GetFilesInFolder(Guid folderId)
            {
                try
                {
                    var query = new GetFilesInFolderQuery { FolderId = folderId };
                    var files = await _mediator.Send(query);
                    return Ok(files);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Failed to get files: {ex.Message}");
                }
            }
        }
    }


