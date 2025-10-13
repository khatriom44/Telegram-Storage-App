using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Models
{
    public class FileMetadataMessage
    {
        public string Type { get; set; } = "FILE_METADATA";
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public Guid FolderId { get; set; }
        public bool IsChunked { get; set; }
        public List<string> ChunkTelegramIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
