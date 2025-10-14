using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITelegramService
    {
        Task<List<Folder>> GetAllFoldersAsync();
        Task<Folder?> GetFolderByIdAsync(Guid folderId);
        Task CreateFolderAsync(Folder folder);
        Task UpdateFolderAsync(Folder folder);
        Task DeleteFolderAsync(Guid folderId);
        Task<bool> TestConnectionAsync();
        Task<List<FileItem>> GetFilesInFolderAsync(Guid folderId);

    }
}
