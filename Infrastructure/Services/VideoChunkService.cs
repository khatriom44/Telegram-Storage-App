using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class VideoChunkService : IVideoChunkService
    {
        public async Task<List<Stream>> SplitVideoAsync(Stream videoStream, int chunkSizeMb)
        {
            // This is a placeholder implementation
            // In reality, you would use FFMpegCore to split videos
            // For now, we'll do simple binary chunking

            var chunks = new List<Stream>();
            var chunkSize = chunkSizeMb * 1024 * 1024; // Convert MB to bytes
            var buffer = new byte[chunkSize];

            videoStream.Position = 0;

            while (videoStream.Position < videoStream.Length)
            {
                var bytesRead = await videoStream.ReadAsync(buffer, 0, chunkSize);
                if (bytesRead > 0)
                {
                    var chunkStream = new MemoryStream();
                    await chunkStream.WriteAsync(buffer, 0, bytesRead);
                    chunkStream.Position = 0;
                    chunks.Add(chunkStream);
                }
            }

            return chunks;
        }

        public async Task<Stream> MergeVideoChunksAsync(List<Stream> chunks)
        {
            var mergedStream = new MemoryStream();

            foreach (var chunk in chunks)
            {
                chunk.Position = 0;
                await chunk.CopyToAsync(mergedStream);
            }

            mergedStream.Position = 0;
            return mergedStream;
        }
    }
}
