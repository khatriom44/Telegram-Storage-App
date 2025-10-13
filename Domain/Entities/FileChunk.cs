using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class FileChunk
    {
        public Guid Id { get; set; }
        public Guid FileId { get; set; }
        public int Index { get; set; }
        public string TelegramFileId { get; set; }
        public long Size { get; set; }
        public string ChecksumMD5 { get; set; }
    }
}
