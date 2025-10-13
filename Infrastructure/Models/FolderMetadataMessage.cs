using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class FolderMetadataMessage
    {
        public string Type { get; set; } = "FOLDER_METADATA";
        public Guid FolderId { get; set; }
        public string FolderName { get; set; }
        public Guid? ParentFolderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
