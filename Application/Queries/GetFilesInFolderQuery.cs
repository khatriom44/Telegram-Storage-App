using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainFile = Domain.Entities.File; // Use correct namespace


namespace Application.Queries
{
    public class GetFilesInFolderQuery : IRequest<List<DomainFile>>
    {
        public Guid FolderId { get; set; }
    }
}
