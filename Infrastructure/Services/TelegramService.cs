using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Web.Configurations;

namespace Infrastructure.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly TelegramBotClient _botClient;
        private readonly TelegramSettings _settings;

        public TelegramService(TelegramBotClient botClient, IOptions<TelegramSettings> options)
        {
            _botClient = botClient;
            _settings = options.Value;
        }

        public async Task CreateFolderAsync(Folder folder)
        {
            // Serialize folder metadata to JSON string
            var json = JsonConvert.SerializeObject(folder);

            // Prefix or format message to identify folder metadata messages
            var messageText = "FOLDER:" + json;

            // Send the message to the configured channel
            await _botClient.SendTextMessageAsync(
                chatId: _settings.FileChannelId,
                text: messageText
            );
        }

        public async Task<List<Folder>> GetAllFoldersAsync()
        {
            var folders = new List<Folder>();

            // Telegram API does not provide direct channel message history access,
            // normally you use bot's updates or client libraries like WTelegramClient.
            // This is placeholder logic to illustrate fetching and filtering.

            var messages = await FetchChannelMessagesAsync();

            foreach (var message in messages)
            {
                if (message.Text != null && message.Text.StartsWith("FOLDER:"))
                {
                    var json = message.Text.Substring("FOLDER:".Length);
                    var folder = JsonConvert.DeserializeObject<Folder>(json);
                    if (folder != null)
                        folders.Add(folder);
                }
            }
            return folders;
        }

        public async Task<Folder> GetFolderByIdAsync(Guid folderId)
        {
            var allFolders = await GetAllFoldersAsync();
            return allFolders.FirstOrDefault(f => f.Id == folderId);
        }

        private async Task<IEnumerable<Message>> FetchChannelMessagesAsync()
        {
            // Telegram.Bot library currently does not support fetching full channel history.
            // You may need to use MTProto or WTelegramClient to fetch channel history.

            throw new NotImplementedException("Fetching channel messages must be implemented using MTProto or appropriate Telegram client.");
        }
    }
}
