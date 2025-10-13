using Application.Interfaces;
using Application.Queries;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class GetFoldersInParentQueryHandler : IRequestHandler<GetFoldersInParentQuery, List<Folder>>
    {
        private readonly ITelegramService _service;
        public GetFoldersInParentQueryHandler(ITelegramService service)
        {
            _service = service;
        }
        public async Task<List<Folder>> Handle(GetFoldersInParentQuery request, CancellationToken cancellationToken)
        {
            var allFolders = await _service.GetAllFoldersAsync();
            return allFolders.Where(f => f.ParentFolderId == request.ParentFolderId).ToList();
        }
    }
}
