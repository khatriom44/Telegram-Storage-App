using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IVideoChunkService
    {
        Task<List<Stream>> SplitVideoAsync(Stream videoStream, int chunkSizeMb);
        Task<Stream> MergeVideoChunksAsync(List<Stream> chunks);
    }
}
