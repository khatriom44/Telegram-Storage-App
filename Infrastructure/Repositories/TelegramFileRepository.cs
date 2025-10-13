using Application.Interfaces;
using Infrastructure.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Web.Configurations;

namespace Infrastructure.Repositories
{
    public class TelegramFileRepository : IFileRepository
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _metadataChannelId;

        public TelegramFileRepository(TelegramBotClient botClient, IOptions<TelegramSettings> options)
        {
            _botClient = botClient;
            _metadataChannelId = options.Value.MetadataChannelId;
        }

        public async Task AddAsync(Domain.Entities.File file)
        {
            // Convert domain entity to infrastructure message format
            var metadata = new FileMetadataMessage
            {
                FileId = file.Id,
                FileName = file.Name,
                FileSize = file.Size,
                MimeType = file.MimeType,
                FolderId = file.FolderId,
                IsChunked = file.IsChunked,
                CreatedAt = file.UploadedAt
            };

            var jsonMessage = JsonSerializer.Serialize(metadata);
            await _botClient.SendTextMessageAsync(_metadataChannelId, jsonMessage);
        }

        public async Task<Domain.Entities.File> GetByIdAsync(Guid id)
        {
            // Find Telegram message and convert back to domain entity
            var messages = await GetChannelMessages();

            foreach (var message in messages)
            {
                if (message.Text?.StartsWith("{") == true)
                {
                    try
                    {
                        var metadata = JsonSerializer.Deserialize<FileMetadataMessage>(message.Text);
                        if (metadata?.FileId == id)
                        {
                            return new Domain.Entities.File
                            {
                                Id = metadata.FileId,
                                Name = metadata.FileName,
                                Size = metadata.FileSize,
                                MimeType = metadata.MimeType,
                                FolderId = metadata.FolderId,
                                IsChunked = metadata.IsChunked,
                                TelegramFileId = metadata.ChunkTelegramIds?.FirstOrDefault() ?? "",
                                UploadedAt = metadata.CreatedAt
                            };
                        }
                    }
                    catch (JsonException)
                    {
                        // Skip invalid JSON messages
                        continue;
                    }
                }
            }
            return null;
        }

        public async Task<List<Domain.Entities.File>> GetFilesInFolderAsync(Guid folderId)
        {
            var files = new List<Domain.Entities.File>();
            var messages = await GetChannelMessages();

            foreach (var message in messages)
            {
                if (message.Text?.StartsWith("{") == true)
                {
                    try
                    {
                        var metadata = JsonSerializer.Deserialize<FileMetadataMessage>(message.Text);
                        if (metadata?.Type == "FILE_METADATA" && metadata.FolderId == folderId)
                        {
                            files.Add(new Domain.Entities.File
                            {
                                Id = metadata.FileId,
                                Name = metadata.FileName,
                                Size = metadata.FileSize,
                                MimeType = metadata.MimeType,
                                FolderId = metadata.FolderId,
                                IsChunked = metadata.IsChunked,
                                TelegramFileId = metadata.ChunkTelegramIds?.FirstOrDefault() ?? "",
                                UploadedAt = metadata.CreatedAt
                            });
                        }
                    }
                    catch (JsonException)
                    {
                        // Skip invalid JSON messages
                        continue;
                    }
                }
            }
            return files;
        }

        public async Task DeleteAsync(Guid id)
        {
            // In Telegram, we can't easily delete specific messages without message_id
            // Alternative: Send a "DELETE" marker message
            var deleteMessage = new
            {
                Type = "DELETE_FILE",
                FileId = id,
                DeletedAt = DateTime.UtcNow
            };

            var jsonMessage = JsonSerializer.Serialize(deleteMessage);
            await _botClient.SendTextMessageAsync(_metadataChannelId, jsonMessage);
        }

        private async Task<List<Message>> GetChannelMessages()
        {
            try
            {
                // Get recent updates from the channel
                // Note: This is simplified. In production, you might need to:
                // 1. Use GetChatHistoryAsync if available
                // 2. Implement proper pagination
                // 3. Cache messages for better performance

                var updates = await _botClient.GetUpdatesAsync();
                return updates.Where(u => u.ChannelPost != null &&
                                         u.ChannelPost.Chat.Id.ToString() == _metadataChannelId)
                             .Select(u => u.ChannelPost)
                             .ToList();
            }
            catch (Exception)
            {
                // Handle Telegram API errors
                return new List<Message>();
            }
        }
    }

}
