using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITelegramFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string channelId);
        Task<Stream> DownloadFileAsync(string telegramFileId);

        Task<List<Domain.Entities.File>> GetFilesInFolderAsync(Guid? folderId);

        // Add more if chunking or status required
    }
}
