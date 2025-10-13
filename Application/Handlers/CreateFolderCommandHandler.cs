using Application.Commands;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers
{
    public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, Unit>
    {
        private readonly ITelegramService _telegramService;

        public CreateFolderCommandHandler(ITelegramService telegramService)
        {
            _telegramService = telegramService;
        }

        public async Task<Unit> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
        {
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ParentFolderId = request.ParentFolderId,
                CreatedAt = DateTime.UtcNow
            };
            await _telegramService.CreateFolderAsync(folder);
            return Unit.Value;
        }
    }
}
