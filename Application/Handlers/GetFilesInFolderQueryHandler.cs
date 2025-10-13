using Application.Interfaces;
using Application.Queries;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class GetFilesInFolderQueryHandler : IRequestHandler<GetFilesInFolderQuery, List<Domain.Entities.File>>
    {
        private readonly IFileRepository _fileRepo;
        public GetFilesInFolderQueryHandler(IFileRepository fileRepo)
        {
            _fileRepo = fileRepo;
        }
        public async Task<List<Domain.Entities.File>> Handle(GetFilesInFolderQuery request, CancellationToken cancellationToken)
        {
            return await _fileRepo.GetFilesInFolderAsync(request.FolderId);
        }
    }

}
