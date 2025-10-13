using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Folder
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ParentFolderId { get; set; }
        public List<File> Files { get; set; } = new();
        public List<Folder> Subfolders { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
