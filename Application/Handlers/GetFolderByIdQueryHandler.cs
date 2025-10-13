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
    public class GetFolderByIdQueryHandler : IRequestHandler<GetFolderByIdQuery, Folder>
    {
        private readonly ITelegramService _service;
        public GetFolderByIdQueryHandler(ITelegramService service)
        {
            _service = service;
        }
        public async Task<Folder> Handle(GetFolderByIdQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetFolderByIdAsync(request.FolderId);
        }
    }
}
