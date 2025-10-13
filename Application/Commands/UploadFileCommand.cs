using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    public class UploadFileCommand : IRequest<Guid>
    {
        public string FileName { get; set; }
        public Stream FileStream { get; set; }
        public string MimeType { get; set; }
        public Guid? FolderId { get; set; }
    }
}
