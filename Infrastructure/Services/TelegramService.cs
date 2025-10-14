using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using TL;
using Web.Configurations;
using WTelegram;
using Folder = Domain.Entities.Folder;

namespace Infrastructure.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly TelegramBotClient _botClient;
        private readonly TelegramSettings _settings;
        private readonly ILogger<TelegramService> _logger;
        private readonly Client _mtClient;
        private const string FolderPrefix = "FOLDER:";
        private const string UpdatePrefix = "UPDATE:";
        private const string DeletePrefix = "DELETE:";

        public TelegramService(
            TelegramBotClient botClient,
            IOptions<TelegramSettings> options,
            ILogger<TelegramService> logger)
        {
            _botClient = botClient;
            _settings = options.Value;
            _logger = logger;
            _mtClient = new Client(configProvider: GetConfig);
        }

        // WTelegramClient config callback
        private string GetConfig(string what) =>
            what switch
            {
                "api_id" => _settings.ApiId.ToString(),
                "api_hash" => _settings.ApiHash,
                "phone_number" => _settings.PhoneNumber ?? string.Empty,
                // Let the library handle its defaults for session storage
                _ => null
            };


        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var me = await _botClient.GetMeAsync();
                _logger.LogInformation($"Bot connected: {me.Username}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Telegram Bot");
                return false;
            }
        }

        public async Task CreateFolderAsync(Folder folder)
        {
            try
            {
                folder.CreatedAt = DateTime.UtcNow;
                var payload = FolderPrefix + JsonConvert.SerializeObject(folder);
                var message = await _botClient.SendTextMessageAsync(_settings.ChannelId, payload);

                // Store the message ID for future updates/deletes
                folder.TelegramMessageId = message.MessageId;
                _logger.LogInformation($"Folder created: {folder.Name} (ID: {folder.Id})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create folder: {folder.Name}");
                throw;
            }
        }

        public async Task UpdateFolderAsync(Folder folder)
        {
            try
            {
                folder.UpdatedAt = DateTime.UtcNow;
                var payload = UpdatePrefix + JsonConvert.SerializeObject(folder);
                await _botClient.SendTextMessageAsync(_settings.ChannelId, payload);
                _logger.LogInformation($"Folder updated: {folder.Name} (ID: {folder.Id})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update folder: {folder.Name}");
                throw;
            }
        }

        public async Task DeleteFolderAsync(Guid folderId)
        {
            try
            {
                var payload = $"{DeletePrefix}{folderId}";
                await _botClient.SendTextMessageAsync(_settings.ChannelId, payload);
                _logger.LogInformation($"Folder deleted: {folderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete folder: {folderId}");
                throw;
            }
        }

        public async Task<List<Folder>> GetAllFoldersAsync()
        {
            var history = await _mtClient.Messages_GetHistory(
               peer: new InputPeerChannel(channel_id: (int)_settings.ChannelId, access_hash: 0),
               offset_id: 0,
               offset_date: default(DateTime),  // Use default(DateTime) instead of null or 0
               add_offset: 0,
               limit: _settings.MessageHistoryLimit,
               max_id: 0,
               min_id: 0,
               hash: 0);

            var map = new Dictionary<Guid, Folder>();
            foreach (var msg in history.Messages.OfType<Message>())
            {
                var text = msg.message;
                if (text.StartsWith(FolderPrefix))
                {
                    var folder = JsonConvert.DeserializeObject<Folder>(text.Substring(FolderPrefix.Length));
                    if (folder != null)
                    {
                        folder.TelegramMessageId = msg.id;
                        map[folder.Id] = folder;
                    }
                }
                else if (text.StartsWith(UpdatePrefix))
                {
                    var updated = JsonConvert.DeserializeObject<Folder>(text.Substring(UpdatePrefix.Length));
                    if (updated != null && map.ContainsKey(updated.Id))
                    {
                        map[updated.Id].Name = updated.Name;
                        map[updated.Id].UpdatedAt = updated.UpdatedAt;
                    }
                }
                else if (text.StartsWith(DeletePrefix))
                {
                    var id = Guid.Parse(text.Substring(DeletePrefix.Length));
                    map.Remove(id);
                }
            }

            return map.Values
                      .OrderBy(f => f.CreatedAt)
                      .ToList();
        }

        public async Task<List<FileItem>> GetFilesInFolderAsync(Guid folderId)
        {
            // Fetch raw history
            var history = await _mtClient.Messages_GetHistory(
                peer: new InputPeerChannel(channel_id: (int)_settings.ChannelId, access_hash: 0),
                offset_id: 0, offset_date: default(DateTime), add_offset: 0,
                limit: _settings.MessageHistoryLimit, max_id: 0, min_id: 0, hash: 0);

            var files = new List<FileItem>();
            foreach (var msg in history.Messages.OfType<Message>())
            {
                var text = msg.message;
                if (text.StartsWith("FILE:"))
                {
                    var file = JsonConvert.DeserializeObject<FileItem>(text.Substring("FILE:".Length));
                    if (file != null && file.FolderId == folderId)
                    {
                        file.TelegramMessageId = msg.id;
                        files.Add(file);
                    }
                }
            }
            return files.OrderBy(f => f.UploadedAt).ToList();
        }
        public async Task<Folder?> GetFolderByIdAsync(Guid folderId)
        {
            var folders = await GetAllFoldersAsync();
            return folders.FirstOrDefault(f => f.Id == folderId);
        }
    }
}
