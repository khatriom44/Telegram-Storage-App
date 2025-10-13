using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainFile = Domain.Entities.File; // Use correct namespace


namespace Application.Interfaces
{
    public interface IFileRepository
    {
        Task<DomainFile> GetByIdAsync(Guid id);
        Task<List<DomainFile>> GetFilesInFolderAsync(Guid folderId);
        Task AddAsync(DomainFile file);
        Task DeleteAsync(Guid id);
        // Add other methods as needed
    }
}
