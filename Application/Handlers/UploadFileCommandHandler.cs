using Application.Commands;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Configurations;

namespace Application.Handlers
{
    public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Guid>
    {
        private readonly IFileRepository _fileRepo;
        private readonly ITelegramFileService _telegramService;
        private readonly IVideoChunkService _videoChunkService;
        private readonly TelegramSettings _telegramSettings;


        public UploadFileCommandHandler(
            IFileRepository fileRepo,
            ITelegramFileService telegramService,
            IVideoChunkService videoChunkService,
            IOptions<TelegramSettings> telegramOptions)
        {
            _fileRepo = fileRepo;
            _telegramService = telegramService;
            _videoChunkService = videoChunkService;
            _telegramSettings = telegramOptions.Value;
        }

        public async Task<Guid> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            // Example: If file is video and larger than 2GB, split and upload chunks
            Guid fileId = Guid.NewGuid();
            if (request.MimeType.StartsWith("video") && request.FileStream.Length > 2_000_000_000)
            {
                var chunks = await _videoChunkService.SplitVideoAsync(request.FileStream, 1800); // Example size
                int chunkIndex = 0;
                List<string> telegramFileIds = new();

                foreach (var chunk in chunks)
                {
                    var chunkFileId = await _telegramService.UploadFileAsync(
                        chunk,
                        $"{request.FileName}_part{chunkIndex}",
                        request.MimeType,
                        _telegramSettings.FileChannelId); telegramFileIds.Add(chunkFileId);
                    chunkIndex++;
                }

                // Save metadata for each chunk
                // (Implement logic to save chunk references in repo)
            }
            else
            {
                int maxRetries = 3;
                int retryCount = 0;
                string telegramFileId = null;

                while (retryCount < maxRetries)
                {
                    try
                    {
                        telegramFileId = await _telegramService.UploadFileAsync(
                            request.FileStream,
                            request.FileName,
                            request.MimeType,
                            _telegramSettings.FileChannelId);

                        break; // success exit loop
                    }
                    catch (HttpRequestException ex) when (ex.Message.Contains("timeout"))
                    {
                        retryCount++;
                        await Task.Delay(1000 * retryCount); // wait before retry
                    }
                }

                if (telegramFileId == null)
                {
                    throw new Exception("Upload failed after multiple timeout retries");
                }

                // Save metadata to repo
                var file = new Domain.Entities.File
                {
                    Id = fileId,
                    Name = request.FileName,
                    Size = request.FileStream.Length,
                    MimeType = request.MimeType,
                    TelegramFileId = telegramFileId,
                    UploadedAt = DateTime.UtcNow,
                    FolderId = request.FolderId ?? Guid.Empty,
                    IsChunked = false,
                    ChunkCount = 1
                };
                await _fileRepo.AddAsync(file);
            }
            return fileId;
        }
    }


}
