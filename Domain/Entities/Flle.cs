using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class File
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string MimeType { get; set; }
        public Guid FolderId { get; set; }
        public bool IsChunked { get; set; }
        public int ChunkCount { get; set; }
        public string TelegramFileId { get; set; } // Telegram API storage reference
        public DateTime UploadedAt { get; set; }
    }
}
