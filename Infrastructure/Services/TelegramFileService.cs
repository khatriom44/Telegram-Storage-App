using Application.Interfaces;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using Web.Configurations;

namespace Infrastructure.Services
{
    public class TelegramFileService : ITelegramFileService
    {
        private readonly TelegramBotClient _botClient;
        private readonly string _channelId;

        public TelegramFileService(TelegramBotClient botClient, IOptions<TelegramSettings> options)
        {
            _botClient = botClient;
            _channelId = options.Value.FileChannelId;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string mimeType, string channelId)
        {
            var inputFile = new InputOnlineFile(fileStream, fileName);
            Telegram.Bot.Types.Message message;

            if (mimeType.StartsWith("image/"))
            {
                message = await _botClient.SendPhotoAsync(channelId, inputFile, caption: fileName);
                if (message.Photo != null && message.Photo.Length > 0)
                    return message.Photo[^1].FileId;
            }
            else if (mimeType.StartsWith("video/"))
            {
                message = await _botClient.SendVideoAsync(channelId, inputFile, caption: fileName);
                if (message.Video != null)
                    return message.Video.FileId;
            }
            else if (mimeType.StartsWith("audio/"))
            {
                message = await _botClient.SendAudioAsync(channelId, inputFile, caption: fileName);
                if (message.Audio != null)
                    return message.Audio.FileId;
            }
            else if (mimeType == "application/x-shockwave-flash"
                     || mimeType == "application/x-flash-video"
                     || fileName.EndsWith(".swf"))
            {
                message = await _botClient.SendAnimationAsync(channelId, inputFile, caption: fileName);
                if (message.Animation != null)
                    return message.Animation.FileId;
            }
            else
            {
                message = await _botClient.SendDocumentAsync(channelId, inputFile, caption: fileName);
                if (message.Document != null)
                    return message.Document.FileId;
            }

            throw new Exception("Upload failed: Could not determine file type or Telegram returned no file ID.");
        }


        public async Task<Stream> DownloadFileAsync(string telegramFileId)
        {
            var file = await _botClient.GetFileAsync(telegramFileId);
            var stream = new MemoryStream();
            await _botClient.DownloadFileAsync(file.FilePath, stream);
            stream.Position = 0;
            return stream;
        }

        public Task<List<Domain.Entities.File>> GetFilesInFolderAsync(Guid? folderId)
        {
            throw new NotImplementedException();
        }
    }
}
