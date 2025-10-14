using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class FolderDetailsViewModel
    {
        // The folder metadata
        public Folder Folder { get; set; } = default!;

        // List of files within this folder
        public List<Domain.Entities.FileItem> Files { get; set; } = new();
    }
}
